
// 用户Session数据
class UserSession
{
    constructor(socket, packetHandler)
    {
        this.m_Socket = socket;
        this.m_packetHandler = packetHandler;
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
}

module.exports = UserSession;