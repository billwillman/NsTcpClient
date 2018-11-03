var NetManager = require("./Core/NetManager");
var DefaultPacketHandler = require("./Core/DefaultPacketHandler");
var TestMessage = require("./Test/TestMessage");


var gateServer = null;
function RegisterServerMessage()
{
    if (gateServer == null)
        return;
    gateServer.RegisterServerMessage(0, new TestMessage());
}



function Main() {

    gateServer = new NetManager();
    gateServer.SetPacketHandlerClass(DefaultPacketHandler);
    RegisterServerMessage();
    gateServer.Listen(1024);
}

// 主循环
Main();