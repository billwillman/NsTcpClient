var UdpServer = require("./UdpServer");
var KcpObj = require("./Kcp");

// KCP的Server
class KcpServer extends UdpServer
{
    constructor(packetHandleClass, kcpId, kcpMode, wndSize)
    {
        super(packetHandleClass, 0);
        this.m_Kcp = new KcpObj(this, kcpId, kcpMode, wndSize);
    }

    /*------------------------------- Kcp回调 -------------------------*/

    _OnKcpRecvMessage(recv, info)
    {
        super._OnMessage(recv, info);
    }

    _OnKcpSendMessage(msg, size, context)
    {
        if (this.m_Socket == null || msg == null || size <= 0 || 
            context == null || context.address == null || context.port == null)
            return;

        this.m_Socket.send(msg, 0, size, context.port, context.address, 
                (err, bytes)=>
                {});
    }

    /*----------------------------------------------------------------*/
    /*--------------------------覆盖方法-------------------------------*/
    Close()
    {
        super.Close();
        if (this.m_Kcp != null)
        {
            this.m_Kcp.Close();
        }
    }

    _OnMessage(msg, clientInfo)
    {
        // 传给KCP
        this.m_KcpObj.OnMessage(msg, clientInfo);
    }

    Send(ip, port, packetHandle, buf, bufOffset, sendSize)
    {
        if (ip == null || this.m_PacketHandle == null || packetHandle == null || 
            port == null || buf == null || !Buffer.isBuffer(buf))
            return false;

        var sendBuf = this.m_PacketHandle.GeneratorSendBuf(packetHandle, buf, bufOffset, sendSize);
        if (sendBuf == null)
            return false;

        return this.m_KcpObj.Send(ip, port, sendBuf);
    }
}

module.exports = KcpServer;