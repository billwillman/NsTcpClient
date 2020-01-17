capnp compile -o csharp Msg.proto > Msg.cs

pause

capnp decode Msg.proto LoginMsg < test.bin > message.txt