/*
  DB服务器
*/

var NetManager = require("./NetManager");
var DefaultPacketHandler = require("./DefaultPacketHandler");
var RegisterDBMessage = require("./RegisterDBMessage");

function DBServer(port)
{
    this.Init();
}

DBServer.prototype = NetManager.prototype;
DBServer.prototype.constructor = DBServer;

// 创建DB服务器
DBServer.Create =
  function (port)
  {
        if (port == null)
          return null;
        var server = new DBServer(port);
        server.SetPacketHandlerClass(DefaultPacketHandler);

        new RegisterDBMessage();

        server.Listen(port);
        return server;
  }

module.exports = DBServer;