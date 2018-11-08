var AbstractServerMessage = require("../../Server/Core/AbstractServerMessage");

class S_HeartMessage extends AbstractServerMessage
{
    constructor()
    {
        super();
    }

    DoRecv(targetSocket)
    {
        // 这个就是心跳包
        this.SendMessage(1000);
    }
}

module.exports = S_HeartMessage;