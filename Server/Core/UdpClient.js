const dgram = require('dgram');
var AbstractMessageMgr = require("./AbstractMessageMgr");

class UdpClient
{
    constructor(packetHandleClass, isIpv6)
    {
        this.m_Socket = null;
        this.m_ServerListener = null;
        this.m_Listener = listener;
        this.m_IsIpv6 = isIpv6;
        this.m_DefaultServerMsgListener = null;
        this.m_LastRemoteIp = null;
        this.m_LastRemotePort = null;
        if (packetHandleClass != null)
        {
            this.m_PacketHandle = new packetHandleClass(this, recvBufSize);
        }
    }

    SetListener(listener)
    {
        this.m_Listener = listener;
    }

    RegisterDefaultServerMsgListener(listener)
    {
        if (listener == null)
            listener = new AbstractMessageMgr();
        this.m_DefaultServerMsgListener = listener;
    }

    RegisterDefaultSrvAbstractMsg(headerId, abstractMsg)
    {
        if (headerId == null || abstractMsg == null)
            return;
        if (this.m_DefaultServerMsgListener == null)
            this.m_DefaultServerMsgListener = new AbstractMessageMgr();
        this.m_DefaultServerMsgListener.RegisterSrvMsg.call(this.m_DefaultServerMsgListener, headerId, abstractMsg);
    }

    _OnDefaultMessageHandle(packet, remoteInfo)
    {
        if (this.m_DefaultServerMsgListener == null || this.m_DefaultServerMsgListener.OnMessage == null)
            return;
        this.m_DefaultServerMsgListener.OnMessage.call(this.m_DefaultServerMsgListener, packet, remoteInfo, this);
    }

    Close()
    {
        if (this.m_Socket != null)
        {
            try
            {
                this.m_Socket.close();
            } catch (except)
            {}
            this.m_Socket = null;
        }
        
        this.m_LastRemoteIp = null;
        this.m_LastRemotePort = null;
    }

    _OnSendError(err, bytes)
    {
        try
        {
            if (this.m_Listener != null)
            {   
                this.m_Listener.OnSendErrorEvent.call(this.m_Listener, err, bytes);
            }
        } catch (except)
        {}

        this.Close();
    }

    _OnError(err)
    {
        try
        {
            if (this.m_Listener != null)
            {   
                this.m_Listener.OnErrorEvent.call(this.m_Listener, err);
            }
        } catch (except)
        {}

        this.Close();
    }

    _OnClose()
    {
        if (this.m_Listener != null)
        {   
            this.m_Listener.OnCloseEvent.call(this.m_Listener);
        }
    }

    _OnMessage(msg, info)
    {
        if (this.m_PacketHandle == null || this.m_PacketHandle.OnPacketRead == null)
            return;
        var packet = this.m_PacketHandle.UdpBufToPacket.call(this.m_PacketHandle, msg);
        if (packet == null)
            return;
        this._SendPacketRead(packet, info);
    }

    RegisterServerMessage(headerId, serverMsgListener)
    {
        if (headerId == null || serverMsgListener == null)
            return;
        if (this.m_ServerListener == null)
            this.m_ServerListener = {};
        this.m_ServerListener[headerId] = serverMsgListener;
    }

    _SendPacketRead(packet, remoteInfo)
    {
        if (packet == null)
            return;
        if (this.m_ServerListener != null && packet.header != null)
        {
            var headerId = packet.header.header;
            var serverMsgListener = this.m_ServerListener[headerId];
            if (serverMsgListener != null && serverMsgListener.OnMessage != null)
            {
                serverMsgListener.OnMessage.call(serverMsgListener, packet, remoteInfo, this);
                return;
            }
        }

        this._OnDefaultMessageHandle(packet, remoteInfo);
    }

    // 支持直接发送AbstractMessage
    SendMessage(ip, port, packetHandle, message)
    {
        if (ip == null || port == null || message == null || packetHandle == null)
            return false;
        var buf = null;
        if (message != null)
        {
            message.DoSend();
            buf = message.m_Buf;
        }

        return this.Send(ip, port, buf);
    }

    _CreateSocket(ip, port)
    {
        if (ip == null || port == null)
            return;

        if (this.m_LastRemoteIp != ip || this.m_LastRemotePort != port)
        {
            this.Close();
            this.m_LastRemoteIp = ip;
            this.m_LastRemotePort = port;
        }

        if (this.m_Socket == null)
        {
            var udpType = this.m_IsIpv6 ? "udp6": "udp4";
            this.m_Socket = dgram.createSocket(udpType);
            this.m_Socket.on("error",
                (err)=>
                {
                    this._OnError(err);
                });
            this.m_Socket.on("close",
                ()=>
                {
                    this._OnClose();
                });

            this.m_Socket.on("message",
                (msg, info)=>
                {
                    this._OnMessage(msg, info);
                });
        }
    }

    Send(ip, port, packetHandle, buf, bufOffset, sendSize)
    {
        if (ip == null || this.m_PacketHandle == null || packetHandle == null || 
            port == null || buf == null || !Buffer.isBuffer(buf))
            return false;

        this._CreateSocket(ip, port);

        var sendBuf = this.m_PacketHandle.GeneratorSendBuf(packetHandle, buf, bufOffset, sendSize);
        if (sendBuf == null)
            return false;

        this.m_Socket.send(sendBuf, port, ip, 
            (err, bytes)=>
            {
                this._OnSendError(err, bytes);
            });
        
        return true;
    }
}

module.exports = UdpClient;