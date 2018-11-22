protoc --js_out=import_style=commonjs,binary:. testProto.proto
protoc --csharp_out=./ testProto.proto