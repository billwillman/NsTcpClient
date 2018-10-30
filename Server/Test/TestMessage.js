
var IServerMessageListener = require("../Core/IServerMessageListener");
var NetManager = require("../Core/NetManager")

function TestMessage()
{
    var inst = NetManager.GetInstance();
    inst.RegisterServerMessage(0, this);
}

TestMessage.prototype = IServerMessageListener.prototype;
TestMessage.prototype.OnMessage =
    function (packet)
    {
        if (packet.data != null)
        {
            var str = packet.data.toString("utf8");
            if (str != null)
                console.log (str);
        }
    }

module.exports = TestMessage;
