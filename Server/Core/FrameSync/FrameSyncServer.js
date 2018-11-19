/*
    帧同步服务器
*/

/*
流程：
    1. 客户端和服务器先TCP连接，然后TCP告诉客户端服务器UDP, KCP的端口号等信息
    2. 客户端正式发送正式帧同步数据给服务器，同时UDP, KCP, TCP同时发送
    3. 服务器只负责转发，拿到数据全部转发操作。
    4. 如果TCP连接断了，则认为网络就断了，需要客户端进行重连操作。
    5. UDP或者Kcp心跳包，服务器应答超过一定时间没有应答则认为网络UDP不通
    6. 帧同步包需要有排序好的列表，方便取；因为TCP和KCP是顺序的，但UDP不一定是顺序的，
       所以UDP还需要一个缓存的UDP链表，超过多少直接抛弃最早进入的。
*/

var NetManager = require("../NetManager");
var KcpServer = require("../KcpServer");
var UdpServer = require("../UdpServer");
var DefaultPacketHandler = require("../DefaultPacketHandler");
require("../Kcp");

// 三个服务器接收
class FrameSyncServer extends NetManager
{
    constructor()
    {
        super();
        this.SetPacketHandlerClass(DefaultPacketHandler);
        // kcpId=1983
        this.m_KcpServer = new KcpServer(DefaultPacketHandler, 1983, KCPMode.quick);
        this.m_KcpServer.SetListener(this);

        this.m_ClientMap = {};

        // 初始化协议处理
        this._InitPackets();
    }

    _InitPackets()
    {

    }

    FindSessionById(clientId)
    {
        if (clientId == null)
            return null;
        var session = this.m_ClientMap[clientId];
        return session;
    }

    // 注册收到消息处理
    RegisterSrvMessageClass(packetId, messageClass)
    {
        if (packetId == null || messageClass == null)
            return;
        // Tcp注册
        this.RegisterDefaultSrvAbstractMsg(packetId, messageClass);
        // Kcp注册
        this.m_KcpServer.RegisterDefaultSrvAbstractMsg(packetId, messageClass);
    }

    // 转发所有
    SendAllChannelMessage(packetId, message, clientId)
    {
        if (packetId == null || clientId == null)
            return false;

        var userSession = this.FindSessionById(clientId);
        if (userSession == null)
            return false;
        this.SendChannelMessage(0, packetId, message, clientSocket);
        var kcpClient = userSession.kcpClient;
        if (kcpClient != null)
        {
            this.SendChannelMessage(1, packetId, message, kcpClient);
        }
        var udpClient = userSession.udpClient;
        if (udpClient != null)
        {
            this.SendChannelMessage(2, packetId, message, udpClient);
        }
    }

    // 发送通道协议
    SendChannelMessage(channelId, packetId, message, client)
    {
        if (channelId == null || packetId == null || client == null)
            return false;
        switch (channelId)
        {
            case 0:
                // TCP消息
                return this.SendMessage(packetId, message, null, client);
            case 1:
                // Kcp消息
                return this.m_KcpServer.SendMessage(client, packetId, message);
        }

        return false;
    }

    OnRemoveSessionEvent(session)
    {
        if (session == null)
            return;
        var globalId = session.globalId;
        if (globalId != null)
        {
            if (this.m_ClientMap != null)
            {
                if (this.m_ClientMap[globalId] != null)
                {
                    delete this.m_ClientMap[globalId];
                }
            }
        }
    }

    OnAddSessionEvent(session)
    {
        if (session == null)
            return;
        var globalId = FrameSyncServer.m_GlobalClientId++;
        session.globalId = globalId;
        if (this.m_ClientMap == null)
            this.m_ClientMap = {};
        this.m_ClientMap[globalId] = session;
    }

    // 关闭状态
    Close()
    {
        super.Close();
        if (this.m_KcpServer != null)
        {
            this.m_KcpServer.Close();
        }
        this.m_ClientMap = null;
    }


    // 运行起来
    Run()
    {
        // 侦听事件
        this.Listen(1983);
        if (this.m_KcpServer != null)
        {
            this.m_KcpServer.StartListen(1984, false);
        }
    }

    _SendFrameGroup(frameGroup)
    {
        if (frameGroup == null)
            return;
    }
}

FrameSyncServer.m_GlobalClientId = 0;

module.exports = FrameSyncServer;