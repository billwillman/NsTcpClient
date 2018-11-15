/*
使用KCP的UdpClient
*/

// KCP库
const kcp = require('node-kcp');

var UdpClient = require("./UdpClient");
require("./Kcp.js");

class KcpClient extends UdpClient
{
    // kcp默认的wndSize是32，我这边默认是64
    constructor(packetHandleClass, isIpv6, kcpId, kcpMode, wndSize)
    {
        super(packetHandleClass, isIpv6);
        this.m_kcpId = kcpId;
        if (this.m_kcpId  == null)
            this.m_kcpId  = 0;
        this.m_Kcp = null;
        this.m_KcpMode = kcpMode;
        this.m_WndSize = wndSize;
        if (this.m_WndSize == null)
            this.m_WndSize = 64;
        if (this.m_KcpMode == null)
            this.m_KcpMode = KCPMode.quick;
        this.m_KcpTimer = null;
        this.m_LastKcpTimerTick = -1;
    }

    _OnCheckTimerCallBack(info)
    {
        if (this.m_Kcp != null)
            {
                this.m_Kcp.update(Date.now());

                // 看是否Recv了
                var recv = this.m_Kcp.recv();
                if (recv != null)
                {
                    super._OnMessage(recv, info);
                    this._StartCheckTimer(KCPInternal, info);
                } else
                {
                    // 查看CHECK时间
                    var newInteral = this.m_Kcp.check();
                    this._StartCheckTimer(newInteral, info);
                }
            }
            else {
                    clearInterval(this.m_KcpTimer);
                    this.m_KcpTimer = null;
                }
    }

    _StartCheckTimer(internal, info)
    {
        if (this.m_KcpTimer != null)
        {

            if (internal != null && this.m_LastKcpTimerTick == internal)
            {
                this.m_KcpTimer.refresh();
                return;
            }

            clearInterval(this.m_KcpTimer);
            this.m_KcpTimer = null;
        }
        if (internal == null)
            return;
            
        this.m_LastKcpTimerTick = internal;
        this.m_KcpTimer = setInterval((info)=>
        {
            this._OnCheckTimerCallBack(info);
        }, internal, info);
    }

    _CheckKcp(ip, port)
    {
        if (this.m_Kcp == null || this.m_Kcp.context.address != ip || this.m_Kcp.context.port != port)
         {
            var context = {"address": ip, "port": port};
            this.m_Kcp = new kcp.KCP(this.m_kcpId, context);
            this.m_Kcp.context = context;
            if (this.m_KcpMode == KCPMode.quick)
                this.m_Kcp.nodelay(1, 10, 2, 1);
            else
                this.m_Kcp.nodelay(0, 40, 0, 0);
            // 设置滑动窗口
            this.m_Kcp.wndsize(this.m_WndSize, this.m_WndSize);
            // 发送时回调
            this.m_Kcp.output(
                (data, size, context)=>
                {
                    this._OnKcpSendMessage(data, size, context);
                });
        }
    }

    _CheckKcp(info)
    {
        if (info == null)
            return;
        this._CheckKcp(info.address, info.port);
    }

    // 接收入口
    _OnMessage(msg, info)
    {
        // 检查KCP
        this._CheckKcp(info);

        // 接收数据
        this.m_Kcp.input(msg);

        // 默认10ms调用
        this._StartCheckTimer(KCPInternal, info);
    }

    // 发送入口
    Send(ip, port, packetHandle, buf, bufOffset, sendSize)
    {
        if (ip == null || this.m_PacketHandle == null || packetHandle == null || 
            port == null || buf == null || !Buffer.isBuffer(buf))
            return false;

        this._CreateSocket(ip, port);
        this._CheckKcp(ip, port);

        if (this.m_Kcp == null)
            return false;

        var sendBuf = this.m_PacketHandle.GeneratorSendBuf(packetHandle, buf, bufOffset, sendSize);
        if (sendBuf == null)
            return false;

        var ret = this.m_Kcp.send(sendBuf);
        if (ret == 0)
            this._StartCheckTimer(KCPInternal, this.m_Kcp.context);
        
        return ret == 0;
    }

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
        this.m_Kcp == null;
        if (this.m_KcpTimer != null)
        {
            clearInterval(this.m_KcpTimer);
            this.m_KcpTimer = null;
        }
        this.m_LastKcpTimerTick = -1;
    }
}

module.exports = KcpClient;