/*
TCP客户端连接
*/

TcpClientStatus = {
    None = 0,
    Connecting = 1,
    Connected = 2,
    ConnectFailed = 3,
    ConnectAbort = 4
}

function TcpClient(packetHandle)
{
    this.m_ServerIp = null;
    this.m_ServerPort = null;
    this.m_PacketHandle = packetHandle;
    this.m_Status = TcpClientStatus.None;
    this.m_Socket = null;
}

TcpClient.prototype.constructor = TcpClient;

TcpClient.prototype.IsConnected = 
    function ()
    {
        return this.GetStatus() == TcpClientStatus.Connected;
    }

TcpClient.prototype.GetStatus = 
    function ()
    {
        if (this.m_Socket == null || this.m_Status == null)
            return TcpClientStatus.None;
        return this.m_Status;
    }


TcpClient.prototype.GetServerIp = 
    function ()
    {
        return this.m_ServerIp;
    };

TcpClient.prototype.GetServerPort =
    function ()
    {
        return this.m_ServerPort;
    }

TcpClient.prototype.DisConnect =
    function ()
    {
        if (this.m_Socket == null)
            return;
        
    }

TcpClient.prototype.ConnectServer = 
    function(serverIp, serverPort)
    {
        if (serverIp == null || serverPort == null)
            return;
        // 关闭当前连接
        this.DisConnect();
        this.m_ServerIp = serverIp;
        this.m_ServerPort = serverPort;
        // 连接服务器
    }

TcpClient.prototype.SendBuf =
    function (packetHandle, data)
    {
        if (packetHandle == null || this.m_Socket == null || 
            this.m_Status == null || this.m_PacketHandle == null)
            return false;
        if (this.m_Status != TcpClientStatus.Connected)
        {
            return false;
        }
        return this.m_PacketHandle.SendBuf(this.m_Socket, packetHandle, data);
    }

module.exports = TcpClient;