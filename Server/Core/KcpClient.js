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

    _OnKcpMessage(msg, size, context)
    {
        super._OnMessage(msg, context);
    }

    _OnCheckTimerCallBack()
    {
        if (this.m_Kcp != null)
            {
                this.m_Kcp.update(Date.now());

                // 查看CHECK时间
                var newInteral = this.m_Kcp.check();
                this._StartCheckTimer(newInteral);
            }
            else {
                    clearInterval(this.m_KcpTimer);
                    this.m_KcpTimer = null;
                }
    }

    _StartCheckTimer(internal)
    {
        if (this.m_KcpTimer != null)
        {

            if (internal != null && m_LastKcpTimerTick == internal)
            {
                this.m_KcpTimer.refresh();
                return;
            }

            clearInterval(this.m_KcpTimer);
            this.m_KcpTimer = null;
        }
        if (internal == null)
            return;
        this.m_KcpTimer = setInterval(()=>
        {
            this._OnCheckTimerCallBack();
        }, internal);
    }

    _OnMessage(msg, info)
    {
        if (this.m_Kcp == null || this.m_Kcp.context.address != info.address || this.m_Kcp.port != info.port)
        {
            var context = {"address": info.address, "port": info.port};
            this.m_Kcp = new kcp.KCP(this.m_kcpId, context);
            this.m_Kcp.context = context;
            if (this.m_KcpMode == KCPMode.quick)
                this.m_Kcp.nodelay(1, 10, 2, 1);
            else
                this.m_Kcp.nodelay(0, 40, 0, 0);
            // 设置滑动窗口
            this.m_Kcp.wndsize(this.m_WndSize, this.m_WndSize);
            this.m_Kcp.output(
                (data, size, context)=>
                {
                    this._OnKcpMessage(data, size, context);
                });
        }

        this.m_Kcp.input(msg);

        // 10ms调用
        this._StartCheckTimer(10);
    }
}

module.exports = KcpClient;