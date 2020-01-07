# Cap'n Proto CMake Package Configuration
#
# When configured and installed, this file enables client projects to find Cap'n Proto using
# CMake's find_package() command. It adds imported targets in the CapnProto:: namespace, such as
# CapnProto::kj, CapnProto::capnp, etc. (one target for each file in pkgconfig/*.pc.in), defines
# the capnp_generate_cpp() function, and exposes some variables for compatibility with the original
# FindCapnProto.cmake module.
#
# Example usage:
#   find_package(CapnProto)
#   capnp_generate_cpp(CAPNP_SRCS CAPNP_HDRS schema.capnp)
#   add_executable(foo main.cpp ${CAPNP_SRCS})
#   target_link_libraries(foo PRIVATE CapnProto::capnp)
#   target_include_directories(foo PRIVATE ${CMAKE_CURRENT_BINARY_DIR})
#
# If you are using RPC features, use 'CapnProto::capnp-rpc' in the target_link_libraries() call.
#
# Paths to `capnp` and `capnpc-c++` are exposed in the following variables:
#   CAPNP_EXECUTABLE
#       Path to the `capnp` tool (can be set to override).
#   CAPNPC_CXX_EXECUTABLE
#       Path to the `capnpc-c++` tool (can be set to override).
#
# For FindCapnProto.cmake compatibility, the following variables are also provided. Please prefer
# using the imported targets in new CMake code.
#   CAPNP_INCLUDE_DIRS
#       Include directories for the library's headers.
#   CANP_LIBRARIES
#       The Cap'n Proto library paths.
#   CAPNP_LIBRARIES_LITE
#       Paths to only the 'lite' libraries.
#   CAPNP_DEFINITIONS
#       Compiler definitions required for building with the library.
#   CAPNP_FOUND
#       Set if the libraries have been located (prefer using CapnProto_FOUND in new code).
#

####### Expanded from @PACKAGE_INIT@ by configure_package_config_file() #######
####### Any changes to this file will be overwritten by the next CMake run ####
####### The input file was CapnProtoConfig.cmake.in                            ########

get_filename_component(PACKAGE_PREFIX_DIR "${CMAKE_CURRENT_LIST_DIR}/../../../" ABSOLUTE)

macro(set_and_check _var _file)
  set(${_var} "${_file}")
  if(NOT EXISTS "${_file}")
    message(FATAL_ERROR "File or directory ${_file} referenced by variable ${_var} does not exist !")
  endif()
endmacro()

macro(check_required_components _NAME)
  foreach(comp ${${_NAME}_FIND_COMPONENTS})
    if(NOT ${_NAME}_${comp}_FOUND)
      if(${_NAME}_FIND_REQUIRED_${comp})
        set(${_NAME}_FOUND FALSE)
      endif()
    endif()
  endforeach()
endmacro()

####################################################################################

set(CapnProto_VERSION 0.8-dev)

set(CAPNP_EXECUTABLE $<TARGET_FILE:CapnProto::capnp_tool>
    CACHE FILEPATH "Location of capnp executable")
set(CAPNPC_CXX_EXECUTABLE $<TARGET_FILE:CapnProto::capnpc_cpp>
    CACHE FILEPATH "Location of capnpc-c++ executable")
set(CAPNP_INCLUDE_DIRECTORY "${PACKAGE_PREFIX_DIR}/include")

# work around http://public.kitware.com/Bug/view.php?id=15258
if(NOT _IMPORT_PREFIX)
  set(_IMPORT_PREFIX ${PACKAGE_PREFIX_DIR})
endif()



include("${CMAKE_CURRENT_LIST_DIR}/CapnProtoTargets.cmake")
include("${CMAKE_CURRENT_LIST_DIR}/CapnProtoMacros.cmake")


# FindCapnProto.cmake provides dependency information via several CAPNP_-prefixed variables. New
# code should not rely on these variables, but prefer linking directly to the imported targets we
# now provide. However, we should still set these variables to ease the transition for projects
# which currently depend on the find-module.

set(CAPNP_INCLUDE_DIRS ${CAPNP_INCLUDE_DIRECTORY})

# No need to list all libraries, just the leaves of the dependency tree.
set(CAPNP_LIBRARIES_LITE CapnProto::capnp)
set(CAPNP_LIBRARIES CapnProto::capnp-rpc CapnProto::capnp-json
                    CapnProto::kj-http)

set(CAPNP_DEFINITIONS)
if(TARGET CapnProto::capnp AND NOT TARGET CapnProto::capnp-rpc)
  set(CAPNP_DEFINITIONS -DCAPNP_LITE)
endif()

set(CAPNP_FOUND ${CapnProto_FOUND})
