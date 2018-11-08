/*
登陆服务器
 */
var NetManager = require("./NetManager");
var DefaultPacketHandler = require("./DefaultPacketHandler");
var RegisterLoginMessage = require("./RegisterLoginMessage");

class LoginServer extends NetManager
{
    constructor(port)
    {
        super();
    }
}

 LoginServer.Create = 
    function (port)
    {
        if (port == null)
            return null;
        var server = new LoginServer(port);
        server.SetPacketHandlerClass(DefaultPacketHandler);

        // 注册登陆服务器消息
        new RegisterLoginMessage();

        server.Listen(port);
        return server;
    }

 module.exports = LoginServer;