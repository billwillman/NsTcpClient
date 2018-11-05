/*
游戏逻辑服务器
*/
var NetManager = require("./NetManager");
var RegisterGameMessage = require("./RegisterGameMessage");
var DefaultPacketHandler = require("./DefaultPacketHandler");

function GameServer(id, port)
{
    this.Init();
    this.m_Id = id;
}

GameServer.prototype = NetManager.prototype;
GameServer.prototype.constructor = GameServer;

GameServer.Create = function (id, port)
{
    if (id == null || port == null)
        return null;

    var server = new GameServer(id, port);
    server.SetPacketHandlerClass(DefaultPacketHandler);

    // 注冊消息
    new RegisterGameMessage(this);

    server.Listen(port);
    return server;
}

module.exports = GameServer;