
var LinkedList = require("./struct/LinkedListNode");

class IUserListener
{
    Update()
    {}
}

// 用户Session数据
class UserSession
{
    constructor(socket, packetHandler, listener)
    {
        this.m_Socket = socket;
        this.m_packetHandler = packetHandler;
        this.m_LinkedListNode = new LinkedListNode(this);
        this.m_Listener = listener;
    }

    Close()
    {
        if (this.m_Socket != null)
        {
            try
            {
                this.m_Socket.destroy();
            } catch (except)
            {
            
            }
            this.m_Socket = null;
        }
    }

    HandleMessage(data)
    {
        if (this.m_packetHandler == null)
            return;
        this.m_packetHandler.OnPacketRead.call(this.m_packetHandler, data, this.m_Socket);
    }

    SendBuf(packetHandle, buf, args)   
    {
        if (packetHandle == null || this.m_packetHandler == null || this.m_Socket == null) 
            return false;
        return this.m_packetHandler.SendBuf.call(this.m_packetHandler, this.m_Socket, packetHandle, buf, args);
    }

    GetLinkedListNode()
    {
        return this.m_LinkedListNode;
    }

    // 更新
    Update()
    {
        if (this.m_Listener != null)
            this.m_Listener.Update.call(this.m_Listener);
    }
}

UserSession.MaxUpdateCount = 100;

module.exports = UserSession;