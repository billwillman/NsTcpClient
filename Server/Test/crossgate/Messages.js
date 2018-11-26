var AbstractServerMessage = require("../../Core/AbstractServerMessage");
var AbstractClientMessage = require("../../Core/AbstractClientMessage");

// 发送协议
class USER_ENTER_PACK_KEY_NOTIFY extends AbstractClientMessage
{
    constructor()
    {
        super();
    }

    DoSend()
    {
        this.WriteString("");
    }
}

class USER_PACK_KEY_NOTIFY extends AbstractClientMessage
{
    constructor()
    {
        super();
    }

    DoSend()
    {
        this.WriteString("");
    }
}

class C_SEND_LOGIN_USER__ReadyLoginQueue extends AbstractClientMessage
{
    DoSend()
    {
        this.WriteString("{" +
            "\"platform\":0," +
            "\open_id\":\"qq\"," +
            "\"system\":100," +
            "\"program\":1," +
            "\"res\":\"1.0.0.0\"," +
            "\"config\":\"0.0.2041\"," +
            "\"serverid\":2," +
            "\"reconnect\":false," +
            "\"heartbeat_disconnect\":false," +
            "\"Network\":\"wifi\"," +
            "\"SystemHardware\":\"\"," +
            "\"TelecomOper\":\"(无)\"," +
            "\"XinYueVip\":false" +
            "}");
        this.WriteString("serverId");
        this.WriteString("");
        this.WriteString("");
        this.WriteString("127.0.0.1");
        this.WriteString("8001");
        this.WriteString("");
        this.WriteUInt(0);
        this.WriteByte(2);
        this.WriteByte(0);
    }
}

class G_SEND_LOGIN_CHECKACCOUNT  extends AbstractClientMessage
{
    DoSend()
    {
        this.WriteUInt(0);
    }
}

class G_SEND_LOGIN_QUERYROLELIST extends AbstractClientMessage
{
    DoSend()
    {
        this.WriteUInt(0);
        this.WriteByte(1);
        
        this.WriteString("莫名的悲傷");
        this.WriteUInt(1);
        this.WriteByte(1);
        this.WriteUInt16(1);
        this.WriteUInt16(1);
        this.WriteUInt16(1);
        this.WriteByte(1);
        this.WriteByte(1);
        this.WriteUInt16(100);
        this.WriteUInt16(100);
        this.WriteUInt16(100);
        this.WriteUInt16(100);
        this.WriteUInt16(100);
        this.WriteByte(1);
        this.WriteByte(1);
        this.WriteByte(1);
        this.WriteByte(1);
        this.WriteUInt16(1);
        this.WriteByte(1);
    }
}

// 接收
class C_RECEIVE_LOGIN_USER__ReadyLoginQueue extends AbstractServerMessage
{
    constructor()
    {
        super();
    }

    DoRecv(clientSocket)
    {
        this.SendMessage(1001, new C_SEND_LOGIN_USER__ReadyLoginQueue(), clientSocket);
        this.SendMessage(1201, new G_SEND_LOGIN_QUERYROLELIST(), clientSocket);
    }
}

class G_RECV_LOGIN_CHECKACCOUNT extends AbstractServerMessage
{
    constructor()
    {
        super();
    }

    DoRecv(clientSocket)
    {
        this.SendMessage(1100, new G_SEND_LOGIN_CHECKACCOUNT(), clientSocket);
    }
}

var crossgateMessages = {};
crossgateMessages.USER_ENTER_PACK_KEY_NOTIFY = USER_ENTER_PACK_KEY_NOTIFY;
crossgateMessages.C_SEND_LOGIN_USER__ReadyLoginQueue = C_SEND_LOGIN_USER__ReadyLoginQueue;
crossgateMessages.C_RECEIVE_LOGIN_USER__ReadyLoginQueue = C_RECEIVE_LOGIN_USER__ReadyLoginQueue;
crossgateMessages.USER_PACK_KEY_NOTIFY = USER_PACK_KEY_NOTIFY;
crossgateMessages.G_RECV_LOGIN_CHECKACCOUNT = G_RECV_LOGIN_CHECKACCOUNT;
crossgateMessages.G_SEND_LOGIN_CHECKACCOUNT = G_SEND_LOGIN_CHECKACCOUNT;
crossgateMessages.G_SEND_LOGIN_CHECKACCOUNT = G_SEND_LOGIN_CHECKACCOUNT;
crossgateMessages.G_SEND_LOGIN_QUERYROLELIST = G_SEND_LOGIN_QUERYROLELIST;

module.exports = crossgateMessages;