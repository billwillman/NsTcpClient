
var DefaultPacketHandler = require("../Server/Core/DefaultPacketHandler");
var AbstractMessageMgr = require("../Server/Core/AbstractMessageMgr");
var TcpClient = require("../Server/Core/TcpClient");
var C_User_Login = require("./Messages/C_User_Login");

function TestClient()
{
     this.m_TcpClient = new TcpClient(DefaultPacketHandler, this);
     this.m_TcpClient.ConnectServer("127.0.0.1", 1024);   
}

TestClient.prototype = AbstractMessageMgr.prototype;
TestClient.prototype.constructor = TestClient;
TestClient.prototype.OnConnectEvent = 
    function (sucess)
    {
        if (sucess)
        {
            console.info("连接成功");
            // 发送第一个协议包
            this.SendLoginMsg();
        }
        else
        {
            console.error("连接失败");
        }
    }

TestClient.prototype.Run = 
    function ()
    {
        //while (true)
        {

        }
    }

TestClient.prototype.SendLoginMsg =
    function ()
    {
        this.m_TcpClient.SendMessage(1, new C_User_Login("zengyi", "123"));
    }

module.exports = TestClient;