
const dgram = require('dgram');

class UdpServer
{
    constructor(packetHandleClass)
    {
        this.m_Socket = null;
        this.m_Listener = null;
        this.m_DefaultServerMsgListener = null;
        if (packetHandleClass != null)
        {
            this.m_PacketHandle = new packetHandleClass(this, 0);
        }
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

    _OnDefaultMessageHandle(packet, clientInfo)
    {
        if (this.m_DefaultServerMsgListener == null || this.m_DefaultServerMsgListener.OnMessage == null)
            return;
        this.m_DefaultServerMsgListener.OnMessage.call(this.m_DefaultServerMsgListener, packet, clientInfo, this);
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
    }

    SetListener(listener)
    {
        this.m_Listener = listener;
    }

    _OnError(err)
    {
        if (this.m_Listener != null)
            this.m_Listener.OnErrorEvent.call(this.m_Listener, err);
    }

    _OnListening()
    {
        if (this.m_Listener != null)
            this.m_Listener.OnStartListeningEvent.call(this.m_Listener);
    }

    _SendPacketRead(packet, clientInfo)
    {
        if (packet == null)
            return;

        if (this.m_Listener != null && this.m_Listener.OnMessagePacket != null)
        {
            try
            {
                this.m_Listener.OnMessagePacket.call(this.m_Listener, packet, clientInfo);
            } catch (except)
            {}
        }
        
        if (this.m_ServerListener != null && packet.header != null)
        {
            var headerId = packet.header.header;
            var serverMsgListener = this.m_ServerListener[headerId];
            if (serverMsgListener != null && serverMsgListener.OnMessage != null)
            {
                serverMsgListener.OnMessage.call(serverMsgListener, packet, clientInfo, this);
                return;
            }
        }

        this._OnDefaultMessageHandle(packet, clientInfo);
    }

    _OnMessage(msg, clientInfo)
    {
        if (this.m_PacketHandle == null || this.m_PacketHandle.OnPacketRead == null)
            return;
        var packet = this.m_PacketHandle.UdpBufToPacket.call(this.m_PacketHandle, msg);
        if (packet == null)
            return;

        if (this.m_Listener != null && this.m_Listener.OnMessage != null)
        {
            this.m_Listener.OnMessage.call(this.m_Listener, msg, clientInfo);
        }
        
        this._SendPacketRead(packet, clientInfo);
    }

    _OnClose()
    {

    }

    GetAddress()
    {
        if (this.m_Socket != null)
        {
            return this.m_Socket.address();
        }
        return null;
    }

    SetBroadCast(isEnabled)
    {
        if (isEnabled == null)
            return false;

        if (this.m_Socket != null)
        {
            this.m_Socket.setBroadcast(isEnabled);
            return true;
        }
        return false;
    }

    // 开始UDP监听
    StartListen(port, isIpv6)
    {
        this.Close();
        if (port == null)
            return false;

        if (isIpv6 == null)
            isIpv6 = false;

        // udp类型
        var udpType = isIpv6 ? "udp6" : "udp4";

        this.m_Socket = dgram.createSocket(udpType);
        this.m_Socket.on("error",
            (err)=>
            {
                this._OnError(err);
            });

        this.m_Socket.on("listening",
            ()=>
            {
                this._OnListening();
            });

        this.m_Socket.on("message",
            (msg, info)=>
            {
                this._OnMessage(msg, info);
            });

        this.m_Socket.on("close",
            ()=>
            {
                this._OnClose();
            });

        this.m_Socket.bind(port);
        
        return true;
    }


    Send(clientInfo, packetHandle, buf, bufOffset, sendSize)
    {
        if (clientInfo == null)
            return false;
        return this.Send(clientInfo.address, clientInfo.port, packetHandle, buf, bufOffset, sendSize);
    }

    Send(ip, port, packetHandle, buf, bufOffset, sendSize)
    {
        if (ip == null || this.m_PacketHandle == null || packetHandle == null || 
            port == null || buf == null || !Buffer.isBuffer(buf))
            return false;
    
        var sendBuf = this.m_PacketHandle.GeneratorSendBuf(packetHandle, buf, bufOffset, sendSize);
        if (sendBuf == null)
            return false;

        this.m_Socket.send(sendBuf, port, ip, (err, bytes)=>
            {});
        return true;
    }

    SendMessage(clientInfo, packetHandle, message)
    {
        if (clientInfo == null || clientInfo.address == null || clientInfo.port == null || packetHandle == null)
            return false;
        return this.SendMessage(clientInfo.address, clientInfo.port, packetHandle, message);
    }

    SendMessage(ip, port, packetHandle, message)
    {
        if (ip == null || port == null || packetHandle == null)
            return false;
        
        return true;
    }
}

module.exports = UdpServer;