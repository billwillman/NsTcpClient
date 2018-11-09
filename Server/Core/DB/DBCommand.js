/*
   DB队列数据
*/

// DB命令
class DBCommand
{
    constructor(key, sql, paramArgs)
    {
        this.m_Key = key;
        this.m_sql = sql;
        this.m_Node = null;
        this.m_ParamArgs = paramArgs;
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

    GetLinkedNode()
    {
        return this.m_Node;
    }
}

module.exports = DBCommand;