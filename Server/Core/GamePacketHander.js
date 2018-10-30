
function GamePacketHander(buf, readSize, offset)
{

    if (buf == null || !Buffer.isBuffer(buf)) {
        this.Reset();
        return;
    }
    if (offset == null)
        offset = 0;
    var mustSize = GamePacketHander.Size;
    var len = readSize - buf.byteOffset;
    if (len < mustSize) {
        this.Reset();
        return;
    }
    this.dataSize = buf.readInt32LE(0);
    this.headerCrc32 = buf.readUInt32LE(1);
    this.dataCrc32 = buf.readUInt32LE(2);
    this.header = buf.readInt32LE(3);
}

GamePacketHander.prototype.Reset =
    function ()
    {
        this.dataSize = 0;
        this.headerCrc32 = 0;
        this.dataCrc32 = 0;
        this.header = 0;
    }

GamePacketHander.prototype.constructor = GamePacketHander;

GamePacketHander.Size = 16;


module.exports = GamePacketHander