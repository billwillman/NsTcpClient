


// 用户Session数据
function UserSession(socket, packetHandler)
{
    this.m_Socket = socket;
    this.m_packetHandler = packetHandler;
}

UserSession.prototype.constructor = UserSession;


UserSession.prototype.Close = function ()
{
    if (this.m_Socket != null)
    {
        try
        {
            this.m_Socket.destroy();
        } catch
        {
            
        }
        this.m_Socket = null;
    }
}

UserSession.prototype.HandleMessage =
    function (data)
    {
        if (this.m_packetHandler == null)
            return;
        this.m_packetHandler.OnPacketRead.call(this.m_packetHandler, data, this.m_Socket);
    }

UserSession.prototype.SendBuf = 
    function (packetHandle, buf, args)
    {
        if (packetHandle == null || this.m_packetHandler == null || this.m_Socket == null) 
            return false;
        return this.m_packetHandler.SendBuf.call(this.m_packetHandler, this.m_Socket, packetHandle, buf, args);
    }


module.exports = UserSession;