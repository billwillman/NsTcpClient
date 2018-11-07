
var AbstractMessageMgr = require("./AbstractMessageMgr");

function RegisterGateMessage(netMgr, isGSGate)
{
    // 是否可以转发的ID
    this.m_CrossIdMap = {};
    netMgr.RegisterDefaultServerMsgListener.call(netMgr, this);
    if (isGSGate)
    {
        // GameServer的Gate
    } else
    {
        // LoginServer的Gate
    }
}

RegisterGateMessage.prototype = AbstractMessageMgr.prototype;
RegisterGateMessage.prototype.constructor = RegisterGateMessage;

var superOnMessage = RegisterGateMessage.prototype.OnMessage;
RegisterGateMessage.prototype.OnMessage = 
    function (packet, clientSocket, netMgr)
    {
        var isDone = superOnMessage.call(this, packet, clientSocket, netMgr);
        if (!isDone)
        {
            // 判断是否可以分发
            if (this.m_CrossIdMap != null)
            {
                var headerId = packet.header.header;
                if (headerId != null)
                {
                    var isCanCross = this.m_CrossIdMap[headerId];
                    if (isCanCross != null && isCanCross)
                    {
                        // 可以转发
                        this.SendToServer(packet, clientSocket, netMgr);
                    }
                }
            }
        }
    }

// 转发给GS或者LS
RegisterGateMessage.prototype.SendToServer =
    function (packet, clientSocket, netMgr)
    {

    }

// 注册可以转发的协议ID
RegisterGateMessage.prototype.RegisterCrossId =
    function (headerId)
    {
        if (headerId == null)
            return;
        if (this.m_CrossIdMap == null)
            this.m_CrossIdMap = {};
        this.m_CrossIdMap[headerId] = true;
    }



module.exports = RegisterGateMessage;