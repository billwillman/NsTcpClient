
var IServerMessageListener = require("../Core/IServerMessageListener");
var NetManager = require("../Core/NetManager")

function TestMessage()
{}

TestMessage.prototype = IServerMessageListener.prototype;
TestMessage.prototype.constructor = TestMessage;

TestMessage.prototype.OnMessage =
    function (packet, clientSocket, netMgr)
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

module.exports = TestMessage;
