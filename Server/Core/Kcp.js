// KCP库
const kcp = require('node-kcp');

KCPMode =
{
    "quick": 0,
    "normal": 1
}

KCPInternal = 10;

// Kcp对象
class KcpObj
{
    constructor(parent, kcpId, kcpMode, wndSize)
    {
        this.m_Parent = parent;
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
                    this.m_Parent._OnKcpRecvMessage(recv, info);
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
                    // 发送给上层
                    this.m_Parent._OnKcpSendMessage(data, size, context);
                });
        }
    }

    _CheckKcp(info)
    {
        if (info == null)
            return;
        this._CheckKcp(info.address, info.port);
    }

    /*---------------------------------- 外部调用接口 -------------------------*/
    
    Close()
    {
        this.m_Kcp == null;
        if (this.m_KcpTimer != null)
        {
            clearInterval(this.m_KcpTimer);
            this.m_KcpTimer = null;
        }
        this.m_LastKcpTimerTick = -1;
    }

    OnMessage(msg, info)
    {
        // 检查KCP
        this._CheckKcp(info);

        // 接收数据
        this.m_Kcp.input(msg);

        // 默认10ms调用
        this._StartCheckTimer(KCPInternal, info);
    }


    Send(ip, port, sendBuf)
    {
        if (ip == null || port == null || sendBuf == null || !Buffer.isBuffer(sendBuf))
            return false;
        this._CheckKcp(ip, port);

        if (this.m_Kcp == null)
            return false;

        var sendResult = this.m_Kcp.send(sendBuf);
        var ret = sendResult == 0;
        if (ret)
            this._StartCheckTimer(KCPInternal, this.m_Kcp.context);
        return ret;
    }
}

module.exports = KcpObj;