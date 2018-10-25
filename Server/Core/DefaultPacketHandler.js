
var AbstractPacketHandler = require("./AbstractPacketHandler")
var GamePacketHander = require("./GamePacketHander");

function DefaultPacketHandler()
{
    this.m_RecvSize = 0;
}

// 继承
DefaultPacketHandler.prototype = AbstractPacketHandler.prototype;

DefaultPacketHandler.prototype.constructor = DefaultPacketHandler;
DefaultPacketHandler.prototype.OnPacketRead =
    function (data)
    {
        if (data == null ||  !Buffer.isBuffer(data))
            return;
        // 粘包处理
        /*
        var recvsize = data.length;
        if (recvsize > 0)
        {
            this.m_RecvSize += recvsize;
            var recvBufSz = this.m_RecvSize;
            var i = 0;
            var header = new GamePacketHander(data);
            var headerSize = GamePacketHander.GetSize();
            while (recvBufSz - i >= headerSize)
            {
                if (recvBufSz - i < header.dataSize + headerSize)
                    break;

            }
        }

        */
    }

DefaultPacketHandler.prototype.SendBuffer =
    function (buf)
    {

        return true;
    }


module.exports = DefaultPacketHandler