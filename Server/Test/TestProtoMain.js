
var NetManager = require ("../Core/NetManager");
var AbsractServerMessage = require("../Core/AbstractServerMessage");
var DefaultPacketHandler = require("../Core/DefaultPacketHandler");
var ProtoBufMgr = require("../Core/ProtoBufMgr");

class TestProtoMessage extends AbsractServerMessage
{
    constructor()
    {
        super();
    }

    DoRecv(clientSocket)
    {
        var message = this.GetProtoMessageObject();
        console.log(message);
        var userName = message.UserName;
        var password = message.Password;

        // 新发送
        var newMsg = this.NewProtoMessageById(100);
        newMsg.setUsername(userName);
        newMsg.setPassword(password);
        this.SendProtoMessage(100, newMsg, clientSocket);
    }
}

class TestProtoServer extends NetManager
{
    constructor()
    {
        super();
        this.RegisterDefaultSrvAbstractMsg(100, TestProtoMessage);
    }
}


function Main()
{

    ProtoBufMgr.GetInstance().RegisterPacketId_ProtoJs(100, "./Messages/Proto/testProto_pb", "C_S_Login_Req");

    var server = new TestProtoServer();
    server.SetPacketHandlerClass(DefaultPacketHandler);
    server.Listen(1024);
}

Main();