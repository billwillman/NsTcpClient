var NetManager = require("./Core/NetManager");
var DefaultPacketHandler = require("./Core/DefaultPacketHandler");
var TestMessage = require("./Test/TestMessage")

function RegisterServerMessage()
{
    new TestMessage();
}

function Main() {

    var inst =  NetManager.GetInstance();
    if (inst == null)
        return;
    inst.SetPacketHandlerClass(DefaultPacketHandler);
    RegisterServerMessage();
    inst.Listen(1024);
}

// 主循环
Main();