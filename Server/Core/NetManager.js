var TcpServer = require ("./TcpServer");
var UserSession = require("./UserSession");

function NetManager() {
    this.m_TcpServer = null;
    this.m_SessionMap = null;
}

NetManager.prototype.constructor = NetManager;
NetManager.prototype.m_Instance = null;

NetManager.GetInstance =
    function()
    {
        if (NetManager.m_Instance == null)
        {
            NetManager.m_Instance = new NetManager();
        }
        return NetManager.m_Instance;
    }

NetManager.prototype.Listen =
    function(bindPort)
    {
        if (bindPort == null)
            return false;

        this.Close();
        this.m_TcpServer = new TcpServer(bindPort);

        this.m_TcpServer.SetConnectEvent(this.OnClientConnected);

        return this.m_TcpServer.Accept();
    }

NetManager.prototype.Close =
    function ()
    {
        if (this.m_TcpServer != null)
        {
            this.m_TcpServer.Close();
            this.m_TcpServer = null;
        }

        this.m_SessionMap = null;
    }

NetManager.prototype._RemoveSession =
    function(clientSocket)
    {
        if (clientSocket == null)
            return;
        if (this.m_SessionMap != null)
        {
            var userSession = this.m_SessionMap[clientSocket];
            if (userSession != null)
            {
                userSession.Close();
                this.m_SessionMap[clientSocket] = null;
            }
        }
    }

NetManager.prototype.OnClientConnected =
    function (clientSocket)
    {
        if (clientSocket == null)
            return;
        this._RemoveSession(clientSocket);
        this.m_SessionMap[clientSocket] = new UserSession(clientSocket);
    }

module.exports = NetManager;