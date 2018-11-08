
var IServerMessageListener = require("../Core/IServerMessageListener");
var NetManager = require("../Core/NetManager")

class TestMessage extends IServerMessageListener
{
    constructor()
    {
        super();
    }

    OnMessage(packet, clientSocket, netMgr)
    {
        if (packet.data != null)
        {
            var str = packet.data.toString("utf8");
            if (str != null)
                console.log (str);
           // netMgr.CloseClientSocket(clientSocket);
           netMgr.SendBuf(clientSocket, 1);
        }
    }
}

module.exports = TestMessage;
