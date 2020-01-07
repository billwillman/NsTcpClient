// Generated by Cap'n Proto compiler, DO NOT EDIT
// source: test-import2.capnp

#pragma once

#include <capnp/generated-header-support.h>
#include <kj/windows-sanity.h>

#if CAPNP_VERSION != 8000
#error "Version mismatch between generated code and library headers.  You must use the same version of the Cap'n Proto compiler and library."
#endif

#include <capnp/schema.capnp.h>
#include "test-import.capnp.h"
#include "test.capnp.h"

namespace capnp {
namespace schemas {

CAPNP_DECLARE_SCHEMA(f6bd77f100ecb0ff);

}  // namespace schemas
}  // namespace capnp


struct TestImport2 {
  TestImport2() = delete;

  class Reader;
  class Builder;
  class Pipeline;

  struct _capnpPrivate {
    CAPNP_DECLARE_STRUCT_HEADER(f6bd77f100ecb0ff, 0, 3)
    #if !CAPNP_LITE
    static constexpr ::capnp::_::RawBrandedSchema const* brand() { return &schema->defaultBrand; }
    #endif  // !CAPNP_LITE
  };
};

// =======================================================================================

class TestImport2::Reader {
public:
  typedef TestImport2 Reads;

  Reader() = default;
  inline explicit Reader(::capnp::_::StructReader base): _reader(base) {}

  inline ::capnp::MessageSize totalSize() const {
    return _reader.totalSize().asPublic();
  }

#if !CAPNP_LITE
  inline ::kj::StringTree toString() const {
    return ::capnp::_::structString(_reader, *_capnpPrivate::brand());
  }
#endif  // !CAPNP_LITE

  inline bool hasFoo() const;
  inline  ::capnproto_test::capnp::test::TestAllTypes::Reader getFoo() const;

  inline bool hasBar() const;
  inline  ::capnp::schema::Node::Reader getBar() const;

  inline bool hasBaz() const;
  inline  ::TestImport::Reader getBaz() const;

private:
  ::capnp::_::StructReader _reader;
  template <typename, ::capnp::Kind>
  friend struct ::capnp::ToDynamic_;
  template <typename, ::capnp::Kind>
  friend struct ::capnp::_::PointerHelpers;
  template <typename, ::capnp::Kind>
  friend struct ::capnp::List;
  friend class ::capnp::MessageBuilder;
  friend class ::capnp::Orphanage;
};

class TestImport2::Builder {
public:
  typedef TestImport2 Builds;

  Builder() = delete;  // Deleted to discourage incorrect usage.
                       // You can explicitly initialize to nullptr instead.
  inline Builder(decltype(nullptr)) {}
  inline explicit Builder(::capnp::_::StructBuilder base): _builder(base) {}
  inline operator Reader() const { return Reader(_builder.asReader()); }
  inline Reader asReader() const { return *this; }

  inline ::capnp::MessageSize totalSize() const { return asReader().totalSize(); }
#if !CAPNP_LITE
  inline ::kj::StringTree toString() const { return asReader().toString(); }
#endif  // !CAPNP_LITE

  inline bool hasFoo();
  inline  ::capnproto_test::capnp::test::TestAllTypes::Builder getFoo();
  inline void setFoo( ::capnproto_test::capnp::test::TestAllTypes::Reader value);
  inline  ::capnproto_test::capnp::test::TestAllTypes::Builder initFoo();
  inline void adoptFoo(::capnp::Orphan< ::capnproto_test::capnp::test::TestAllTypes>&& value);
  inline ::capnp::Orphan< ::capnproto_test::capnp::test::TestAllTypes> disownFoo();

  inline bool hasBar();
  inline  ::capnp::schema::Node::Builder getBar();
  inline void setBar( ::capnp::schema::Node::Reader value);
  inline  ::capnp::schema::Node::Builder initBar();
  inline void adoptBar(::capnp::Orphan< ::capnp::schema::Node>&& value);
  inline ::capnp::Orphan< ::capnp::schema::Node> disownBar();

  inline bool hasBaz();
  inline  ::TestImport::Builder getBaz();
  inline void setBaz( ::TestImport::Reader value);
  inline  ::TestImport::Builder initBaz();
  inline void adoptBaz(::capnp::Orphan< ::TestImport>&& value);
  inline ::capnp::Orphan< ::TestImport> disownBaz();

private:
  ::capnp::_::StructBuilder _builder;
  template <typename, ::capnp::Kind>
  friend struct ::capnp::ToDynamic_;
  friend class ::capnp::Orphanage;
  template <typename, ::capnp::Kind>
  friend struct ::capnp::_::PointerHelpers;
};

#if !CAPNP_LITE
class TestImport2::Pipeline {
public:
  typedef TestImport2 Pipelines;

  inline Pipeline(decltype(nullptr)): _typeless(nullptr) {}
  inline explicit Pipeline(::capnp::AnyPointer::Pipeline&& typeless)
      : _typeless(kj::mv(typeless)) {}

  inline  ::capnproto_test::capnp::test::TestAllTypes::Pipeline getFoo();
  inline  ::capnp::schema::Node::Pipeline getBar();
  inline  ::TestImport::Pipeline getBaz();
private:
  ::capnp::AnyPointer::Pipeline _typeless;
  friend class ::capnp::PipelineHook;
  template <typename, ::capnp::Kind>
  friend struct ::capnp::ToDynamic_;
};
#endif  // !CAPNP_LITE

// =======================================================================================

inline bool TestImport2::Reader::hasFoo() const {
  return !_reader.getPointerField(
      ::capnp::bounded<0>() * ::capnp::POINTERS).isNull();
}
inline bool TestImport2::Builder::hasFoo() {
  return !_builder.getPointerField(
      ::capnp::bounded<0>() * ::capnp::POINTERS).isNull();
}
inline  ::capnproto_test::capnp::test::TestAllTypes::Reader TestImport2::Reader::getFoo() const {
  return ::capnp::_::PointerHelpers< ::capnproto_test::capnp::test::TestAllTypes>::get(_reader.getPointerField(
      ::capnp::bounded<0>() * ::capnp::POINTERS));
}
inline  ::capnproto_test::capnp::test::TestAllTypes::Builder TestImport2::Builder::getFoo() {
  return ::capnp::_::PointerHelpers< ::capnproto_test::capnp::test::TestAllTypes>::get(_builder.getPointerField(
      ::capnp::bounded<0>() * ::capnp::POINTERS));
}
#if !CAPNP_LITE
inline  ::capnproto_test::capnp::test::TestAllTypes::Pipeline TestImport2::Pipeline::getFoo() {
  return  ::capnproto_test::capnp::test::TestAllTypes::Pipeline(_typeless.getPointerField(0));
}
#endif  // !CAPNP_LITE
inline void TestImport2::Builder::setFoo( ::capnproto_test::capnp::test::TestAllTypes::Reader value) {
  ::capnp::_::PointerHelpers< ::capnproto_test::capnp::test::TestAllTypes>::set(_builder.getPointerField(
      ::capnp::bounded<0>() * ::capnp::POINTERS), value);
}
inline  ::capnproto_test::capnp::test::TestAllTypes::Builder TestImport2::Builder::initFoo() {
  return ::capnp::_::PointerHelpers< ::capnproto_test::capnp::test::TestAllTypes>::init(_builder.getPointerField(
      ::capnp::bounded<0>() * ::capnp::POINTERS));
}
inline void TestImport2::Builder::adoptFoo(
    ::capnp::Orphan< ::capnproto_test::capnp::test::TestAllTypes>&& value) {
  ::capnp::_::PointerHelpers< ::capnproto_test::capnp::test::TestAllTypes>::adopt(_builder.getPointerField(
      ::capnp::bounded<0>() * ::capnp::POINTERS), kj::mv(value));
}
inline ::capnp::Orphan< ::capnproto_test::capnp::test::TestAllTypes> TestImport2::Builder::disownFoo() {
  return ::capnp::_::PointerHelpers< ::capnproto_test::capnp::test::TestAllTypes>::disown(_builder.getPointerField(
      ::capnp::bounded<0>() * ::capnp::POINTERS));
}

inline bool TestImport2::Reader::hasBar() const {
  return !_reader.getPointerField(
      ::capnp::bounded<1>() * ::capnp::POINTERS).isNull();
}
inline bool TestImport2::Builder::hasBar() {
  return !_builder.getPointerField(
      ::capnp::bounded<1>() * ::capnp::POINTERS).isNull();
}
inline  ::capnp::schema::Node::Reader TestImport2::Reader::getBar() const {
  return ::capnp::_::PointerHelpers< ::capnp::schema::Node>::get(_reader.getPointerField(
      ::capnp::bounded<1>() * ::capnp::POINTERS));
}
inline  ::capnp::schema::Node::Builder TestImport2::Builder::getBar() {
  return ::capnp::_::PointerHelpers< ::capnp::schema::Node>::get(_builder.getPointerField(
      ::capnp::bounded<1>() * ::capnp::POINTERS));
}
#if !CAPNP_LITE
inline  ::capnp::schema::Node::Pipeline TestImport2::Pipeline::getBar() {
  return  ::capnp::schema::Node::Pipeline(_typeless.getPointerField(1));
}
#endif  // !CAPNP_LITE
inline void TestImport2::Builder::setBar( ::capnp::schema::Node::Reader value) {
  ::capnp::_::PointerHelpers< ::capnp::schema::Node>::set(_builder.getPointerField(
      ::capnp::bounded<1>() * ::capnp::POINTERS), value);
}
inline  ::capnp::schema::Node::Builder TestImport2::Builder::initBar() {
  return ::capnp::_::PointerHelpers< ::capnp::schema::Node>::init(_builder.getPointerField(
      ::capnp::bounded<1>() * ::capnp::POINTERS));
}
inline void TestImport2::Builder::adoptBar(
    ::capnp::Orphan< ::capnp::schema::Node>&& value) {
  ::capnp::_::PointerHelpers< ::capnp::schema::Node>::adopt(_builder.getPointerField(
      ::capnp::bounded<1>() * ::capnp::POINTERS), kj::mv(value));
}
inline ::capnp::Orphan< ::capnp::schema::Node> TestImport2::Builder::disownBar() {
  return ::capnp::_::PointerHelpers< ::capnp::schema::Node>::disown(_builder.getPointerField(
      ::capnp::bounded<1>() * ::capnp::POINTERS));
}

inline bool TestImport2::Reader::hasBaz() const {
  return !_reader.getPointerField(
      ::capnp::bounded<2>() * ::capnp::POINTERS).isNull();
}
inline bool TestImport2::Builder::hasBaz() {
  return !_builder.getPointerField(
      ::capnp::bounded<2>() * ::capnp::POINTERS).isNull();
}
inline  ::TestImport::Reader TestImport2::Reader::getBaz() const {
  return ::capnp::_::PointerHelpers< ::TestImport>::get(_reader.getPointerField(
      ::capnp::bounded<2>() * ::capnp::POINTERS));
}
inline  ::TestImport::Builder TestImport2::Builder::getBaz() {
  return ::capnp::_::PointerHelpers< ::TestImport>::get(_builder.getPointerField(
      ::capnp::bounded<2>() * ::capnp::POINTERS));
}
#if !CAPNP_LITE
inline  ::TestImport::Pipeline TestImport2::Pipeline::getBaz() {
  return  ::TestImport::Pipeline(_typeless.getPointerField(2));
}
#endif  // !CAPNP_LITE
inline void TestImport2::Builder::setBaz( ::TestImport::Reader value) {
  ::capnp::_::PointerHelpers< ::TestImport>::set(_builder.getPointerField(
      ::capnp::bounded<2>() * ::capnp::POINTERS), value);
}
inline  ::TestImport::Builder TestImport2::Builder::initBaz() {
  return ::capnp::_::PointerHelpers< ::TestImport>::init(_builder.getPointerField(
      ::capnp::bounded<2>() * ::capnp::POINTERS));
}
inline void TestImport2::Builder::adoptBaz(
    ::capnp::Orphan< ::TestImport>&& value) {
  ::capnp::_::PointerHelpers< ::TestImport>::adopt(_builder.getPointerField(
      ::capnp::bounded<2>() * ::capnp::POINTERS), kj::mv(value));
}
inline ::capnp::Orphan< ::TestImport> TestImport2::Builder::disownBaz() {
  return ::capnp::_::PointerHelpers< ::TestImport>::disown(_builder.getPointerField(
      ::capnp::bounded<2>() * ::capnp::POINTERS));
}


