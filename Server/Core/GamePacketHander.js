

class GamePacketHander
{
    constructor(buf, readSize, offset)
    {
        if (buf == null || !Buffer.isBuffer(buf)) {
            this.Reset();
            return;
        }
        var mustSize = GamePacketHander.Size;
    
        if (offset == null)
            offset = 0;
    
        //var offset = buf.byteOffset;
        var len = readSize - offset;
        if (len < mustSize) {
            this.Reset();
            return;
        }
    
        
        this.dataSize = buf.readInt32LE(0 + offset);
        this.headerCrc32 = buf.readUInt32LE(4 + offset);
        this.dataCrc32 = buf.readUInt32LE(8 + offset);
        this.header = buf.readInt32LE(12 + offset);
    }

    Reset()
    {
        this.dataSize = 0;
        this.headerCrc32 = 0;
        this.dataCrc32 = 0;
        this.header = 0;
    }

    ToBuf(buf)
    {
        if (buf == null || !Buffer.isBuffer(buf))
            return false;
        buf.writeInt32LE(this.dataSize, 0);
        buf.writeInt32LE(this.headerCrc32, 4);
        buf.writeInt32LE(this.dataCrc32, 8);
        buf.writeInt32LE(this.header, 12);
        return true;
    }
}

GamePacketHander.Size = 16;


module.exports = GamePacketHander