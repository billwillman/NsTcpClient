var TcpServer = require ("./TcpServer");
var UserSession = require("./UserSession");
var AbstractPacketHandler = require ("./AbstractPacketHandler");
var ITcpServerListener = require("./ITcpServerListener");
var AbstractMessageMgr = require("./AbstractMessageMgr");
var ProtoBufMgr = require("./ProtoBufMgr");
var LinkedList = require("./struct/LinkedList");

class NetManager extends ITcpServerListener
{
    constructor()
    {
        super();
        this.Init();
    }

    Init()
    {
        this.m_TcpServer = null;
        this.m_SessionMap = null;
        this.m_PacketHandlerClass = null;
        this.m_ServerListener = null;
        this.m_DefaultServerMsgListener = null;
        this.m_LinkedList = null;
        this.m_IsEnableUserSessionUpdate = false;
    }

    _SetUserSessionUpdate(isEnable)
    {
        if (isEnable == null)
            isEnable = false;
        this.m_IsEnableUserSessionUpdate = isEnable;
    }

    _UpdateUserSession(netMgr)
    {
        
        if (netMgr == null || netMgr.m_IsEnableUserSessionUpdate == null || !netMgr.m_IsEnableUserSessionUpdate)
            return;
        var list = netMgr.m_LinkedList;
        if (list != null)
        {
            var listCount = list.GetCount();
            var loopCount = UserSession.MaxUpdateCount;
            if (loopCount > listCount)
                loopCount = listCount;
            var index = 0;
            var firstNode = list.GetFirstNode();
            while (firstNode != null)
            {
                var userSession = firstNode.GetValue();
                if (userSession != null)
                    userSession.Update();
                list.AddLastNode(firstNode);
                ++index;
                if (index >= loopCount)
                    break;
            }
        }
        setImmediate(netMgr._UpdateUserSession, netMgr);
     //   process.nextTick();
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

    _OnDefaultMessageHandle(packet, clientSocket)
    {
        if (this.m_DefaultServerMsgListener == null || this.m_DefaultServerMsgListener.OnMessage == null)
            return;
        this.m_DefaultServerMsgListener.OnMessage.call(this.m_DefaultServerMsgListener, packet, clientSocket, this);
    }

    _SendPacketRead(packet, clientSocket)
    {
        if (packet == null)
            return;

        // 根据协议号分发
        if (this.m_ServerListener != null && packet.header != null)
        {
            var headerId = packet.header.header;
            var serverMsgListener = this.m_ServerListener[headerId];
            if (serverMsgListener != null && serverMsgListener.OnMessage != null)
            {
                serverMsgListener.OnMessage.call(serverMsgListener, packet, clientSocket, this);
                return;
            }
        }

        this._OnDefaultMessageHandle(packet, clientSocket);
    }

    RegisterServerMessage(headerId, serverMsgListener)
    {
        if (headerId == null || serverMsgListener == null)
            return;
        if (this.m_ServerListener == null)
            this.m_ServerListener = {};
        this.m_ServerListener[headerId] = serverMsgListener;
    }

    // heartTimeout心跳包
    Listen(bindPort, heartTimeout)
    {
        if (bindPort == null)
            return false;

        this.Close();
        this.m_TcpServer = new TcpServer(bindPort);

        this.m_TcpServer.SetListener.call(this.m_TcpServer, this);

        var ret = this.m_TcpServer.Accept(heartTimeout);

        if (ret)
        {
            this.m_IsEnableUserSessionUpdate = true;
            //process.nextTick(this._UpdateUserSession, this);
            setImmediate(this._UpdateUserSession, this);
        }

        return ret;
    }

    Close()
    {   
        this.m_IsEnableUserSessionUpdate = false;
        if (this.m_TcpServer != null)
        {
            this.m_TcpServer.Close();
            this.m_TcpServer = null;
        }

        this.m_SessionMap = null;
        if (this.m_LinkedList != null)
        {
            this.m_LinkedList.Clear();
        }
    }

    // 关闭所有客户端， 但服务器不停
    CloseAllClientSocket()
    {
        if (this.m_SessionMap != null)
        {
            for (clientSocket in this.m_SessionMap)
            {
                this.CloseClientSocket(clientSocket);
            }
        }
        this.m_SessionMap = null;
        if (this.m_LinkedList != null)
        {
            this.m_LinkedList.Clear();
        }
    }

    OnTimeOut(clientSocket)
    {
        this.CloseClientSocket(clientSocket);
    }

    // 主动断开客户端连接
    CloseClientSocket(clientSocket)
    {
        this._RemoveSession(clientSocket);
    }

    SetPacketHandlerClass(handleClass)
    {
        this.m_PacketHandlerClass = handleClass;
    }

    _RemoveSession(clientSocket)
    {
        if (clientSocket == null)
            return;
        if (this.m_SessionMap != null)
        {
            var id = clientSocket.id;
            var userSession = this.m_SessionMap[id];
            if (userSession != null)
            {
                this.OnRemoveSessionEvent(userSession);
                userSession.Close();
                if (this.m_LinkedList != null)
                {
                    this.m_LinkedList.RemoveNode(userSession.GetLinkedListNode());
                }
                this.m_SessionMap[id] = null;
                delete this.m_SessionMap[id];
            }
        }
    }

    OnRemoveSessionEvent(session)
    {}

    OnAddSessionEvent(session)
    {}

    OnPacketRead(data, socket)
    {
        if (data == null || socket == null)
            return;
        if (this.m_SessionMap == null)
            return;
        var id = socket.id;
        var userSession = this.m_SessionMap[id];
        if (userSession == null)
            return;
        userSession.HandleMessage.call(userSession, data);
    }

    FindSession(clientSocket)
    {
        if (clientSocket == null || this.m_SessionMap == null)
            return null;
        var id = clientSocket.id;
        var ret = this.m_SessionMap[id];
        return ret;
    }

    _GetPacketHandlerClass()
    {
        if (this.m_PacketHandlerClass == null)
            this.m_PacketHandlerClass = AbstractPacketHandler;
        return this.m_PacketHandlerClass;
    }

    OnEndEvent(clientSocket)
    {
        if (clientSocket == null)
                return;
        this._RemoveSession(clientSocket);
    }

    OnConnectedEvent(clientSocket)
    {
        if (clientSocket == null)
            return;
        this._RemoveSession(clientSocket);
        if (this.m_SessionMap == null)
            this.m_SessionMap = {};
        var handlerClass = this._GetPacketHandlerClass();
        var newHandler = new handlerClass(this);
        var session = new UserSession(clientSocket, newHandler);
        // 增加
        var id = ++NetManager.GlobalSocketId;
        clientSocket.id = id;
        this.m_SessionMap[id] = session;
        if (this.m_LinkedList == null)
            this.m_LinkedList = new LinkedList();
        this.m_LinkedList.AddLastNode(session.GetLinkedListNode());
        //-------------
        this.OnAddSessionEvent(session);
    }

    // 发送给客户端
    SendBuf(clientSocket, packetHandle, buf, args)
    {
        if (clientSocket == null || packetHandle == null)
            return false;
        if (this.m_SessionMap == null)
            return false;
        var id = clientSocket.id;
        var session = this.m_SessionMap[id];
        if (session == null)
            return false;
        return session.SendBuf.call(session, packetHandle, buf, args);
    }

    SendMessage(packetHandle, message, args, targetSocket)
    {
        if (targetSocket == null || packetHandle == null)
            return false;
        var buf = null;
        if (message != null)
        {
            message.DoSend();
            buf = message.m_Buf;
        }
        return this.SendBuf(targetSocket, packetHandle, buf, args);
    }

    // 发送Proto包
    SendProtoMessage(packetHandle, message, args, targetSocket)
    {
        if (targetSocket == null || packetHandle == null)
            return false;
        if (message != null)
        {
            var buf = ProtoBufMgr.GetInstance().ProtoMessageToBuf(message);
            if (buf == null)
                return false;
            return this.SendBuf(targetSocket, packetHandle, buf.buffer, args);
        } else
        {
            return this.SendBuf(targetSocket, packetHandle, null, args);
        }
    }

    NewProtoMessageById(packetId)
    {
        return ProtoBufMgr.GetInstance().NewProtoMessageByPacketId(packetId);
    }
}

NetManager.GlobalSocketId = -1;

module.exports = NetManager;