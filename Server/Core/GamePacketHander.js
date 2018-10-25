
function GamePacketHander(buf)
{
    if (buf == null || !Buffer.isBuffer(buf))
        return;
    var mustSize = GamePacketHander.GetSize();
    var len = buf.length - buf.byteOffset;
    if (len < mustSize)
        return;
    this.dataSize = buf.readInt32LE(0);
    this.headerCrc32 = buf.readUInt32LE(1);
    this.dataCrc32 = buf.readUInt32LE(2);
    this.header = buf.readInt32LE(3);
}

GamePacketHeader.GetSize =
    function ()
    {
        return 16;
    }

GamePacketHander.prototype.constructor = GamePacketHander;


module.exports = GamePacketHander