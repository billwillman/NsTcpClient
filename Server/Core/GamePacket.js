var GamePacketHeader = require("./GamePacketHander")

function GamePacket(buf)
{
    if (buf == null || !Buffer.isBuffer(buf))
        return;
    this.header = new GamePacketHeader(buf);
    this.data = Buffer.from(buf, GamePacketHeader.GetSize());
}

GamePacket.prototype.constructor = GamePacket;

module.exports = GamePacket;