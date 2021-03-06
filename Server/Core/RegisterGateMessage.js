
var AbstractMessageMgr = require("./AbstractMessageMgr");
var MessageConsts = require("./MessageConsts");
var  C_HeartMessage = require("./Messages/Client/C_HeartMessage");

class RegisterGateMessage extends AbstractMessageMgr
{
    constructor(netMgr, isGSGate)
    {
        super();
        // 是否可以转发的ID
        this.m_CrossIdMap = {};
        netMgr.RegisterDefaultServerMsgListener.call(netMgr, this);
        this._RegisterClientSendMessages(isGSGate);
    }
    
    // 注册客户端发送过来的协议
     _RegisterClientSendMessages(isGSGate)
    {
        if (isGSGate)
        {
            // GameServer的Gate

            /*----------------------可穿透协议-----------------------*/

            /*-----------------------处理协议------------------------*/
            this.RegisterSrvMsg(MessageConsts.ClientMessage.C_Heart, C_HeartMessage);
        } else
        {
            // LoginServer的Gate
            
            /*----------------------可穿透协议-----------------------*/
           // 用户登录协议
           this.RegisterCrossId(MessageConsts.ClientMessage.C_User_Login, 
                MessageConsts.GateToLSMessage.C_User_Login);
           /*-----------------------处理协议------------------------*/
           this.RegisterSrvMsg(MessageConsts.ClientMessage.C_Heart, C_HeartMessage);
        }
    }

    OnMessage(packet, clientSocket, netMgr)
    {
        var isDone = super.OnMessage(packet, clientSocket, netMgr);
        if (!isDone)
        {
            // 判断是否可以分发
            if (this.m_CrossIdMap != null)
            {
                var headerId = packet.header.header;
                if (headerId != null)
                {
                    var replaceId = this.m_CrossIdMap[headerId];
                    if (replaceId != null)
                    {
                        // 可以转发
                        this.SendToServer(replaceId, packet, clientSocket, netMgr);
                        return true;
                    }
                }
            }
        } else
            return true;
        
        // 发送非法协议, 断开客户端
        netMgr.CloseClientSocket(clientSocket);
    }

    // 转发给GS或者LS
    SendToServer(replaceId, packet, clientSocket, netMgr)
    {
        // 分发
        netMgr.DispatchToServer(replaceId, packet);
    }

    // 注册可以转发的协议ID
    RegisterCrossId(headerId, replaceId)
    {
        if (headerId == null || replaceId == null)
            return;
        if (this.m_CrossIdMap == null)
            this.m_CrossIdMap = {};
        this.m_CrossIdMap[headerId] = replaceId;
    }
}

module.exports = RegisterGateMessage;