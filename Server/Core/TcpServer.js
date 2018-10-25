/*
 * 服务器TcpServer
 * */

var net = require("net");

// ---------------------------------类定义: TcpServer
function TcpServer(bindPort)
{
    this.m_BindPort = bindPort;
    this.m_OnConnectedEvent = null;
    this.m_OnStartListening = null;
    this.m_OnError = null;
    this.m_OnPacketRead = null;
}

TcpServer.prototype.SetConnectEvent =
    function (evt)
    {
        this.m_OnConnectedEvent = evt;
    }

TcpServer.prototype.SetStartListeningEvent =
    function (evt)
    {
        this.m_OnStartListening = evt;
    }

TcpServer.prototype.SetErrorEvent =
    function (evt)
    {
        this.m_OnError = evt;
    }

// 构造函数
TcpServer.prototype.constructor = TcpServer;

// 获得绑定端口
TcpServer.prototype.GetBindPort =
    function()
    {
        return this.m_BindPort;
    }

TcpServer.prototype.GetAddress =
    function()
    {
        if (this.m_Server != null)
        {
            return this.m_Server.address();
        }
        return null;
    }

TcpServer.prototype.Close =
    function ()
    {
        if (this.m_Server != null)
        {
            // 关闭连接
            this.m_Server.close();
            this.m_Server = null;
        }
    }

TcpServer.prototype.SetPacketReadEvent =
    function (evt)
    {
        this.m_OnPacketRead = evt;
    }

// 产生链接时调用
TcpServer.prototype._OnConnected = function (socket)
{
    if (socket != null)
    {
        socket.setNoDelay(true);
        if (this.m_OnConnectedEvent != null)
            this.m_OnConnectedEvent(socket);
    }

    console.log("_OnConnected");
}

// 侦听时候
TcpServer.prototype._OnListening = function ()
{
    if (this.m_OnStartListening != null)
        this.m_OnStartListening();
    console.log("_OnListening");
}

// 出错
TcpServer.prototype._OnError = function (error)
{
    if (this.m_OnError != null)
        this.m_OnError(error);
    console.log("_OnError");
}

TcpServer.prototype._OnPacketRead = function (data)
{
    if (this.m_OnPacketRead != null)
        this.m_OnPacketRead(data);
    console.log("_OnPacketRead");
}

TcpServer.prototype._OnClose = function ()
{
    console.log("_OnClose");
}

// 开始接受网络连接，返回值：是否成功监听
TcpServer.prototype.Accept =
    function ()
    {
        this.Close();
        if (this.m_BindPort == null)
            return false;

        var server = net.createServer();
        this.m_Server = server;
        server.on("connection", this._OnConnected);
        server.on("listening", this._OnListening);
        server.on("error", this._OnError);
        server.on("data", this._OnPacketRead);
        server.on("close", this._OnClose);
        // 侦听事件
        server.listen(this.m_BindPort);
        return true;
    }
//---------------------------------------------------


module.exports = TcpServer;