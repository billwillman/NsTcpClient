var GamePacketHeader = require("./GamePacketHander");

class GamePacket
{
    constructor(header, data)
    {
        this.header = header;
        this.data = data;
    }
}

module.exports = GamePacket;