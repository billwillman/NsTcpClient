/*
 * 心跳包 
*/

var AbstractServerMessage = require("../../AbstractServerMessage");
var MessageConsts = require("../../MessageConsts");


function C_HeartMessage()
{}

C_HeartMessage.prototype = AbstractServerMessage.prototype;
C_HeartMessage.prototype.constructor = C_HeartMessage;

// 客户端发送上来的心跳包
C_HeartMessage.prototype.DoRecv =
    function(clientSocket)
    {
        // 返回心跳包
        this.SendMessage(clientSocket, MessageConsts.SrvToClientMessage.S_Heart);
    }

module.exports = C_HeartMessage;