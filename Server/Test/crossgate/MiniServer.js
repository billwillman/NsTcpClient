/*
魔力的简单登录模块服务器
*/

var NetManager = require("../../Core/NetManager");
var PacketHandleClass = require("./CrossgatePacketHandler");
var Messages = require("./Messages");

class MiniLoginServer extends NetManager
{
    constructor()
    {
        super();
        this.RegisterDefaultSrvAbstractMsg(1001, Messages.C_RECEIVE_LOGIN_USER__ReadyLoginQueue);
    }

    OnAddSessionEvent(session)
    {
        this.SendMessage(1003, Messages.USER_ENTER_PACK_KEY_NOTIFY, session.m_Socket);
    }
}

class MiniGameServer extends NetManager
{
    constructor()
    {
        super();
        this.RegisterDefaultSrvAbstractMsg(1100, Messages.G_RECV_LOGIN_CHECKACCOUNT);
    }

    OnAddSessionEvent(session)
    {
        this.SendMessage(1002, Messages.USER_PACK_KEY_NOTIFY, session.m_Socket);
    }   
}

function Main()
{
    var loginSrv = new MiniLoginServer();
    loginSrv.SetPacketHandlerClass(PacketHandleClass);
    loginSrv.Listen(8000);

    var gameSrv = new MiniGameServer();
    gameSrv.SetPacketHandlerClass(PacketHandleClass);
    gameSrv.Listen(8001);
}

Main();