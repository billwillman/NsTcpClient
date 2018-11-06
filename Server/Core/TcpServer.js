/*
 * 服务器TcpServer
 * */

var net = require("net");

// ---------------------------------类定义: TcpServer
function TcpServer(bindPort)
{
    this.m_BindPort = bindPort;
    this.m_Listener = null;
}

// 构造函数
TcpServer.prototype.constructor = TcpServer;

TcpServer.prototype.SetListener = function (listener)
{
    this.m_Listener = listener;
}


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

// 产生链接时调用
TcpServer.prototype._OnConnected = function (socket)
{
    if (socket != null)
    {
        //socket.setNoDelay(true);
        if (this.m_Listener != null)
            this.m_Listener.OnConnectedEvent.call(this.m_Listener, socket);
    }

    console.log("_OnConnected");
}

// 侦听时候
TcpServer.prototype._OnListening = function ()
{
    if (this.m_Listener != null)
        this.m_Listener.OnStartListeningEvent.call(this.m_Listener);
    console.log("_OnListening");
}

// 出错
TcpServer.prototype._OnError = function (error)
{
    if (this.m_Listener != null)
        this.m_Listener.OnErrorEvent.call(this.m_Listener, error);
    console.log("_OnError");
}

TcpServer.prototype._OnPacketRead = function (data, socket)
{
    if (this.m_Listener != null && socket != null)
        this.m_Listener.OnPacketRead.call(this.m_Listener, data, socket);
}

TcpServer.prototype._OnClose = function ()
{
    if (this.m_Listener != null)
        this.m_Listener.OnCloseEvent.call(this.m_Listener);
    console.log("_OnClose");
}

// 客户端断开连接
TcpServer.prototype._OnEnd = function (socket)
{
    if (this.m_Listener != null)
        this.m_Listener.OnEndEvent.call(this.m_Listener, socket);
    console.log("_OnEnd");
}

TcpServer.prototype._OnTimeOut = function (socket)
{
    if (this.m_Listener != null)
        this.m_Listener.OnTimeOut.call(this.m_Listener, socket);
    console.log("_OnTimeOut");
}

// 开始接受网络连接，返回值：是否成功监听
// timeout为心跳包
TcpServer.prototype.Accept =
    function (heartTimeout)
    {
        this.Close();
        if (this.m_BindPort == null)
            return false;

        var server = net.createServer(
            (socket)=>
            {
                if (socket != null)
                {
                    socket.on("data", 
                    (data) =>
                    {
                        // 获得网络数据
                        this._OnPacketRead(data, socket);
                    }
                    );


                    socket.on("end", 
                    ()=>
                    {
                        this._OnEnd(socket)
                    })
                    ;

                    if (heartTimeout != null)
                    {
                        socket.on("timeout",
                        ()=>
                        {
                            this._OnTimeOut(socket);
                        });
                        socket.setTimeout(heartTimeout);
                    }

                }
            }

        );
        this.m_Server = server;
        
        server.on("connection", 
        (socket)=>
        {
            this._OnConnected(socket);
        }
        );

        server.on("listening", 
        ()=>
        {
            this._OnListening();
        }
        );

        server.on("error", 
        ()=>
        {
            this._OnError();
        }
        );

        server.on("close", 
        ()=>
        {
            this._OnClose();
        }
        );

        // 侦听事件
        server.listen(this.m_BindPort);
        return true;
    }
//---------------------------------------------------


module.exports = TcpServer;