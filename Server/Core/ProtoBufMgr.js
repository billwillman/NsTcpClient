

class ProtoBufMgr
{
    constructor()
    {
        this.m_PbMap = {};
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
                return null;
            }
            if (this.m_PbMap == null)
                this.m_PbMap = {};
            this.m_PbMap[path] = pb;
        }
        return pb;
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
}

ProtoBufMgr.m_Instance = null;
ProtoBufMgr.Instance = function ()
{
    if (ProtoBufMgr.m_Instance == null)
        ProtoBufMgr.m_Instance = new ProtoBufMgr();
    return ProtoBufMgr.m_Instance;
}

model.exports = ProtoBufMgr;