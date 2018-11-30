{
  "targets": [
    {
      "target_name": "recast",
	  "include_dirs": [
	  "<!(node -e \"require('nan')\")",
	  "src/Include"
	  ],
	  "sources": [
	  "src/cpp/recastObj.cc",
	  "src/cpp/node_recast.cc"
	  ],
	  "include_dirs+": [
		"Recast/Include",
		"Detour/Include"],
	  "sources+": [
		"Recast/Source/Recast.cpp",
		"Recast/Source/RecastAlloc.cpp",
		"Recast/Source/RecastArea.cpp",
		"Recast/Source/RecastAssert.cpp",
		"Recast/Source/RecastContour.cpp",
		"Recast/Source/RecastFilter.cpp",
		"Recast/Source/RecastLayers.cpp",
		"Recast/Source/RecastMesh.cpp",
		"Recast/Source/RecastMeshDetail.cpp",
		"Recast/Source/RecastRasterization.cpp",
		"Recast/Source/RecastRegion.cpp",
		
		"Detour/Source/DetourAlloc.cpp",
		"Detour/Source/DetourAssert.cpp",
		"Detour/Source/DetourCommon.cpp",
		"Detour/Source/DetourNavMesh.cpp",
		"Detour/Source/DetourNavMeshBuilder.cpp",
		"Detour/Source/DetourNavMeshQuery.cpp",
		"Detour/Source/DetourNode.cpp"
		]
    }
  ]
}