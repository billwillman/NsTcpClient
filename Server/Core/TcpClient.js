/*
TCP客户端连接
*/
var net = require("net");
var AbstractMessageMgr = require("./AbstractMessageMgr");

TcpClientStatus = {
    None: 0,
    Connecting: 1,
    Connected: 2,
    ConnectFailed: 3,
    ConnectAbort: 4
}

function TcpClient(packetHandleClass, listener, recvBufSize)
{
    this.m_ServerIp = null;
    this.m_ServerPort = null;
    this.m_PacketHandle = null;
    this.m_ServerListener = null;
    if (packetHandleClass != null)
    {
        this.m_PacketHandle = new packetHandleClass(this, recvBufSize);
    }
    this.m_Status = TcpClientStatus.None;
    this.m_Socket = null;
    this.m_Listener = listener;
    // 默认的消息处理对象，只有一个
    this.m_DefaultServerMsgListener = null;
}

TcpClient.prototype.constructor = TcpClient;

TcpClient.prototype.RegisterDefaultServerMsgListener =
    function (listener)
    {
        if (listener == null)
            listener = new AbstractMessageMgr();
        this.m_DefaultServerMsgListener = listener;
    }

TcpClient.prototype.RegisterDefaultSrvAbstractMsg =
    function (headerId, abstractMsg)
    {
        if (headerId == null || abstractMsg == null)
            return;
        if (this.m_DefaultServerMsgListener == null)
            this.m_DefaultServerMsgListener = new AbstractMessageMgr();
        this.m_DefaultServerMsgListener.RegisterSrvMsg.call(this.m_DefaultServerMsgListener, headerId, abstractMsg);
    }

TcpClient.prototype.IsConnected = 
    function ()
    {
        return (this.m_Socket != null) && (this.GetStatus() == TcpClientStatus.Connected);
    }

TcpClient.prototype.IsConnecting =
    function ()
    {
        return (this.m_Socket != null) && (this.GetStatus() == TcpClientStatus.Connecting);
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
            if (this.m_Listener != null && this.m_Listener.OnConnectEvent != null)
                this.m_Listener.OnConnectEvent.call(this.m_Listener, false, this);
        }
        else
        {
            this.m_Status = TcpClientStatus.ConnectAbort;
            if (this.m_Listener != null && this.m_Listener.OnAbortEvent != null)
                this.m_Listener.OnAbortEvent.call(this.m_Listener, this);
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
        if (this.m_Listener != null && this.m_Listener.OnConnectEvent != null)
            this.m_Listener.OnConnectEvent.call(this.m_Listener, true, this);
    }

TcpClient.prototype.ConnectServer = 
    function(serverIp, serverPort, heartTimeout)
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

        this.m_Socket.on("data",
            (data)=>
            {
                this._OnPacketRead(data);
            }
            );
        
        // 心跳包
        if (heartTimeout != null)
        {
            this.m_Socket.on("timeout",
                ()=>
                {
                    this._OnError(null);
                });
            this.m_Socket.setTimeout(heartTimeout);
        }

        this.m_Status = TcpClientStatus.Connecting;
        this.m_Socket.connect(serverPort, serverIp);
        return true;
    }

TcpClient.prototype.SendBuf =
    function (packetHandle, data, args)
    {
        if (packetHandle == null || this.m_Socket == null || 
            this.m_Status == null || this.m_PacketHandle == null)
            return false;
        if (this.m_Status != TcpClientStatus.Connected)
        {
            return false;
        }
        return this.m_PacketHandle.SendBuf(this.m_Socket, packetHandle, data, args);
    }

// 发送消息
TcpClient.prototype.SendMessage =
    function (packetHandle, message, args)
    {
        if (packetHandle == null)
            return false;
        var buf = null;
        if (message != null)
        {
            message.DoSend();
            buf = message.m_Buf;
        }
        return this.SendBuf(packetHandle, buf, args);
    };

TcpClient.prototype.CloseClientSocket =
    function (clientSocket, result)
    {
        /*
        if (result != null && result != -2)
            this._OnError(null);
            */
        this._OnError(null);
    }

TcpClient.prototype._OnDefaultMessageHandle =
    function (packet, clientSocket)
    {
        if (this.m_DefaultServerMsgListener == null || this.m_DefaultServerMsgListener.OnMessage == null)
            return;
        this.m_DefaultServerMsgListener.OnMessage.call(this.m_DefaultServerMsgListener, packet, clientSocket, this);
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
            {
                serverMsgListener.OnMessage.call(serverMsgListener, packet, clientSocket, this);
                return;
            }
        }

        this._OnDefaultMessageHandle(packet, clientSocket);
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