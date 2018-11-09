/*
 GameServer发送给DB SERVER 命令
*/

var AbstractClientMessage = require("../../AbstractClientMessage");
require("../../struct/Utils");
require("../../DB/DBSql");

class GS_DB_Command extends AbstractClientMessage
{
    // 命令ID
    constructor (commandId, clientId, args)
    {
        super();
        this.m_CommandId = commandId;
        this.m_ClientId = clientId;
        this.m_Args = args;
    }

    DoSend()
    {
        // 发送
        this.WriteUInt(commandId);
        this.WriteUInt64(clientId);
        var args = this.m_Args;
        if (args != null && args instanceof Array)
        {
            this.WriteInt(args.length);
            for (var i = 0; i < args.length; ++i)
            {
                var obj = args[i];
                WriteObject(obj);
            }
        }
    }

    WriteObject(obj)
    {
        if (obj == null)
            return;
        var type = typeof(obj);
        if (type == null)
            return;
        if (type == "number")
        {
            var isFloat = _.isFloat(obj);
            if (isFloat)
            {
                this.WriteByte(SqlObjType.NumberFloat);
                this.WriteFloat(obj);
            } else
            {
                this.WriteByte(SqlObjType.NumberInt);
                this.WriteInt64(obj);
            }
        } else if (type == "string")
        {
            this.WriteByte(SqlObjType.String);
            this.WriteString(obj);
        } else
        {
            console.error("GS_DB_Command: type " + type + " not supported!");
        }
    }
}

module.exports = GS_DB_Command;