var GamePacketHeader = require("./GamePacketHander")

function GamePacket(header, data)
{
    this.header = header;
    this.data = data;
}

GamePacket.prototype.constructor = GamePacket;

module.exports = GamePacket;