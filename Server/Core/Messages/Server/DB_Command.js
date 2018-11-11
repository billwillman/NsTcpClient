/*
DB接收消息
*/
var AbstractServerMessage  = require("../../AbstractServerMessage");
require("../../DB/DBSql");

class DB_Command extends AbstractServerMessage
{

    constructor ()
    {
        super();
        this.m_Args = null;
    }
    
    DoRecv(clientSocket)
    {
        var commandId = this.ReadUInt();
        if (commandId == null)
            return;
        var clientId = this.ReadUInt64();
        if (clientId == null)
            return;
        var arrLen = this.ReadInt();
        var sql = DBSql.GetSql(commandId);
        if (sql == null)
            return;
        
        if (arrLen != null && arrLen > 0)
        {
            for (var i = 0; i < arrLen; ++i)
            {
                var type = this.ReadByte();
                switch (type)
                {
                    case SqlObjType.NumberInt:
                        this.PushToArgs(this.ReadInt64());
                        break;
                    case SqlObjType.NumberFloat:
                        this.PushToArgs(this.ReadFloat());
                        break;
                    case SqlObjType.String:
                        this.PushToArgs(this.ReadString());
                        break;
                }
            }
        }

        var netMgr = this.NetManager();
        var key = DBSql.GetCommandKey(clientId, commandId);
        // 压入队列
        netMgr.Push(key, sql, this.m_Args, clientSocket);
    }

    PushToArgs(obj)
    {
        if (obj == null)
            return;
        if (this.m_Args == null)
            this.m_Args = [];
        var idx = this.m_Args.length;
        this.m_Args[idx] = obj;
    }
}

module.exports = DB_Command;