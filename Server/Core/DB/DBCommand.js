/*
   DB队列数据
*/

// DB命令
class DBCommand
{
    constructor(key, sql, paramArgs, targetSocket)
    {
        this.m_Key = key;
        this.m_sql = sql;
        this.m_Node = null;
        this.m_ParamArgs = paramArgs;
        this.m_TargetSocket = targetSocket;
    }

    AttachLinkedNode(linkedNode)
    {
        this.m_Node = linkedNode;
    }

    ChangeParamArgs(paramArgs)
    {
        this.m_ParamArgs = paramArgs;
    }

    GetKey()
    {
        return this.m_Key;
    }

    GetCommand()
    {
        return this.m_sql;
    }

    GetTargetSocket()
    {
        return this.m_TargetSocket;
    }

    GetLinkedNode()
    {
        return this.m_Node;
    }
}

module.exports = DBCommand;