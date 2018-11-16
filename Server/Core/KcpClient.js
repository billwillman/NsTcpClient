/*
使用KCP的UdpClient
*/

var UdpClient = require("./UdpClient");
var KcpObj = require("./Kcp.js");

class KcpClient extends UdpClient
{
    // kcp默认的wndSize是32，我这边默认是64
    constructor(packetHandleClass, isIpv6, kcpId, kcpMode, wndSize)
    {
        super(packetHandleClass, isIpv6);
        this.m_KcpObj = new KcpObj(this, kcpId, kcpMode, wndSize);
    }

    // Kcp接收返回
    _OnKcpRecvMessage(recv, info)
    {
        super._OnMessage(recv, info);
    }

    // 接收入口
    _OnMessage(msg, info)
    {
        this.m_KcpObj.OnMessage(msg, info);
    }

    // 发送入口
    Send(ip, port, packetHandle, buf, bufOffset, sendSize)
    {
        if (ip == null || this.m_PacketHandle == null || packetHandle == null || 
            port == null || buf == null || !Buffer.isBuffer(buf))
            return false;

        this._CreateSocket(ip, port);

        var sendBuf = this.m_PacketHandle.GeneratorSendBuf(packetHandle, buf, bufOffset, sendSize);
        if (sendBuf == null)
            return false;

        return this.m_KcpObj.Send(ip, port, sendBuf);
    }

    // Kcp发送返回
    _OnKcpSendMessage(msg, size, context)
    {
        // UDP发送
        if (this.m_Socket == null || msg == null || size <= 0 || 
            context == null || context.address == null || context.port == null)
            return;
        this.m_Socket.send(msg, 0, size, context.port, context.address, 
            (err, bytes)=>
            {
                this._OnSendError(err, bytes);
            });
    }

    Close()
    {
        super.Close();
        if (this.m_KcpObj != null)
        {
            this.m_KcpObj.Close();
            this.m_KcpObj = null;
        }
    }
}

module.exports = KcpClient;