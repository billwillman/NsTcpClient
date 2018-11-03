/*
TCP客户端连接
*/
var net = require("net");

TcpClientStatus = {
    None = 0,
    Connecting = 1,
    Connected = 2,
    ConnectFailed = 3,
    ConnectAbort = 4
}

function TcpClient(packetHandleClass, listener)
{
    this.m_ServerIp = null;
    this.m_ServerPort = null;
    this.m_PacketHandle = null;
    this.m_ServerListener = null;
    if (packetHandleClass != null)
    {
        this.m_PacketHandle = new packetHandleClass(this);
    }
    this.m_Status = TcpClientStatus.None;
    this.m_Socket = null;
    this.m_Listener = listener;
}

TcpClient.prototype.constructor = TcpClient;

TcpClient.prototype.IsConnected = 
    function ()
    {
        return (this.m_Socket != null) && (this.GetStatus() == TcpClientStatus.Connected);
    }

TcpClient.prototype.GetStatus = 
    function ()
    {
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
       // this.m_Socket.end();
        this.m_Socket.destroy();
        this.m_Socket = null;
        this.m_Status = TcpClientStatus.None;
    }

TcpClient.prototype._OnError = 
    function (error)
    {
        var isConnecting = this.m_Status == TcpClientStatus.Connecting;
        this.DisConnect();
        if (isConnecting)
        {
            this.m_Status = TcpClientStatus.ConnectFailed;
            if (this.m_Listener != null)
                this.m_Listener.OnConnectEvent.call(this.m_Listener, false);
        }
        else
        {
            this.m_Status = TcpClientStatus.ConnectAbort;
            if (this.m_Listener != null)
                this.m_Listener.OnAbortEvent.call(this.m_Listener);
        }
    }

TcpClient.prototype._OnPacketRead =
    function (data)
    {
        if (this.m_PacketHandle == null || this.m_PacketHandle.OnPacketRead == null)
            return;
        this.m_PacketHandle.OnPacketRead.call(this.m_PacketHandle, data, this.m_Socket);
    }

TcpClient.prototype._OnConnect =
    function ()
    {
        this.m_Status = TcpClientStatus.Connected;
        if (this.m_Listener != null)
            this.m_Listener.OnConnectEvent.call(this.m_Listener, true);
    }

TcpClient.prototype.ConnectServer = 
    function(serverIp, serverPort, timeout)
    {
        if (serverIp == null || serverPort == null)
            return false;
        // 关闭当前连接
        this.DisConnect();
        this.m_ServerIp = serverIp;
        this.m_ServerPort = serverPort;
        // 连接服务器
        this.m_Socket = net.Socket();
        if (this.m_Socket == null)
            return false;
        
        this.m_Socket.on("error",
            (error)=>
            {
                this._OnError(error);
            }
        );

        this.m_Socket.on("ready",
            ()=>
            {
                this._OnConnect();
            });

        this.m_Socket.on("timeout",
            ()=>
            {
                this._OnError(null);
            });

        this.m_Socket.on("data",
            (data)=>
            {
                this._OnPacketRead(data);
            }
            );
        
        if (timeout != null)
            this.m_Socket.setTimeout(timeout);

        this.m_Status = TcpClientStatus.Connecting;
        this.m_Socket.connect(serverPort, serverIp);
        return true;
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

TcpClient.prototype.CloseClientSocket =
    function (clientSocket, result)
    {
        if (result != null && result != -2)
            this._OnError(null);
    }

TcpClient.prototype._SendPacketRead =
    function (packet, clientSocket)
    {
        if (packet == null)
            return;
        
        if (this.m_ServerListener != null && packet.header != null)
        {
            var headerId = packet.header.header;
            var serverMsgListener = this.m_ServerListener[headerId];
            if (serverMsgListener != null && serverMsgListener.OnMessage != null)
                serverMsgListener.OnMessage.call(serverMsgListener, packet, clientSocket, this)
        }
    }

TcpClient.prototype.RegisterServerMessage =
    function(headerId, serverMsgListener)
    {
        if (headerId == null || serverMsgListener == null)
            return;
        if (this.m_ServerListener == null)
            this.m_ServerListener = {};
        this.m_ServerListener[headerId] = serverMsgListener;
    }

module.exports = TcpClient;