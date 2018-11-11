/*
用户登录协议
C->GS
*/

var AbstractServerMessage = require("../../AbstractServerMessage");

class C_LoginReq extends AbstractServerMessage
{
    constructor()
    {
        super();
    }

    DoRecv(clientSocket)
    {
        var userName = this.ReadString();
        var password = this.ReadString();
        var netMgr = this.NetManager();
        var clientId = this.m_HeaderCrc32;

        var args = [userName, password];
        if (!netMgr.SendToDBServer(2, clientId, args))
        {
            // 发送返回失败
            netMgr.SendMessage(MessageConsts.FromDBMessage.S_User_LoginRet, 
                new S_C_LoginRep(S_C_LoginRet.DBServerError), [clientId]);
        }
    }
}

module.exports = C_LoginReq;