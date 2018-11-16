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


class FrameSyncServer
{
    constructor()
    {
        // 三个服务器接收
        this.m_TcpServer = new NetManager();
        this.m_KcpServer = new KcpServer(DefaultPacketHandler, 1983, KCPMode.quick);
        this.m_UdpServer = new UdpServer(DefaultPacketHandler);
    }

    _InitPackets()
    {

    }

    // 注册消息处理
    RegisterSrvMessage()
    {

    }
}

module.exports = FrameSyncServer;