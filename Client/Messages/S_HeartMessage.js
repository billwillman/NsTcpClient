var AbstractServerMessage = require("../../Server/Core/AbstractServerMessage");

function S_HeartMessage()
{}

S_HeartMessage.prototype = AbstractServerMessage.prototype;
S_HeartMessage.prototype.constructor = S_HeartMessage;

S_HeartMessage.prototype.DoRecv =
    function(targetSocket)
    {
        // 这个就是心跳包
        this.SendMessage(1000);
    }



module.exports = S_HeartMessage;