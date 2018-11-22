
require("google-protobuf");

/*
function CheckGoog()
{
    if (typeof(goog) == "undefined")
        goog = {};
}
CheckGoog();
*/

class ProtoBufMgr
{
    constructor()
    {
        this.m_PbMap = {};
        this.m_PbJs = {};
    }

    _GetPb(path)
    {
        if (path == null)
            return null;
        var pb = null;
        if (this.m_PbMap != null)
            pb = this.m_PbMap[path];
        if (pb == null)
        {
            try
            {
                var pb = require(path);
                if (pb == null)
                    return null;
            } catch (exception)
            {
                console.error(exception);
                return null;
            }
            if (this.m_PbMap == null)
                this.m_PbMap = {};
            this.m_PbMap[path] = pb;
        }
        return pb;
    }

    NewProtoMessageByPacketId(packetId)
    {
        if (packetId == null)
            return null;
        var messageObj = this._GetMessageObjByPacketId(packetId);
        if (messageObj == null || messageObj.path == null || messageObj.messageName == null)
            return null;
        return this.NewProtoMessage(messageObj.path, messageObj.messageName);
    }

    NewProtoMessage(path, messageName)
    {
        if (path == null || messageName == null)
            return null;
        var pb = this._GetPb(path);
        if (pb == null)
            return null;
        var messageClass = pb[messageName];
        if (messageClass == null)
            return null;
        var message = new messageClass();
        return message;
    }

    ProtoMessageToBuf(message)
    {
        if (message == null || message.serializeBinary == null)
            return null;
        var buf = message.serializeBinary();
        return buf; 
    }

    _GetMessageObjByPacketId(packetId)
    {
        if (packetId == null || this.m_PbJs == null)
            return null;
        return this.m_PbJs[packetId];
    }

    BufToProtoMessageByPacKetId(packetId, buf)
    {
        if (packetId == null || buf == null || !Buffer.isBuffer(buf))
            return null;
        if (this.m_PbJs == null)
            return null;
        var messageObj = this._GetMessageObjByPacketId(packetId);
        if (messageObj == null || messageObj.path == null || messageObj.messageName == null)
            return null;
        return this.BufToProtoMessage(buf, messageObj.path, messageObj.messageName);
    }

    BufToProtoMessage(buf, path, messageName)
    {
        if (buf == null || !Buffer.isBuffer(buf) || 
            path == null || messageName == null)
            return null;
        var pb = this._GetPb(path);
        if (pb == null)
            return null;
        var messageClass = pb[messageName];
        if (messageClass == null || messageClass.deserializeBinary == null)
            return null;
        var message = messageClass.deserializeBinary(buf);
        return message;
    }

    Clear()
    {
        this.m_PbMap = {};
    }

    RegisterPacketId_ProtoJs(packetId, path, messageName)
    {
        if (packetId == null || path == null || messageName == null)
            return false;
        if (this.m_PbJs == null)
            this.m_PbJs = {};
        this.m_PbJs[packetId] = {"path": path, "messageName": messageName};
    }
}

ProtoBufMgr.m_Instance = null;
ProtoBufMgr.GetInstance = function ()
{
    if (ProtoBufMgr.m_Instance == null)
        ProtoBufMgr.m_Instance = new ProtoBufMgr();
    return ProtoBufMgr.m_Instance;
}

module.exports = ProtoBufMgr;