/*
 * 心跳包 
*/

var AbstractServerMessage = require("../../AbstractServerMessage");
var MessageConsts = require("../../MessageConsts");

class C_HeartMessage extends AbstractServerMessage
{
    constructor()
    {
        super();
    }

    // 客户端发送上来的心跳包
    DoRecv(clientSocket)
    {
        // 返回心跳包
        this.SendMessage(MessageConsts.SrvToClientMessage.S_Heart, null, clientSocket);
    }
}

module.exports = C_HeartMessage;