var UdpServer = require("./UdpServer");

// KCP的Server
class KcpServer extends UdpServer
{
    constructor(packetHandleClass)
    {
        super(packetHandleClass);
    }
}

module.exports = KcpServer;