// Gate->Client

var AbstractClientMessage = require("../../AbstractClientMessage");

S_C_LoginRet =
{
    // 成功
    Sucess: 0,
    // DB服务器失败
    DBServerError: 1,
    // 数据库执行失败
    DBSqlError: 2
}

class S_C_LoginRep extends AbstractClientMessage
{
    constructor(loginRet)
    {
        super();
        this.m_LoginRet = loginRet;
    }

    DoSend()
    {
        this.WriteByte(this.m_LoginRet);
    }
}

module.exports = S_C_LoginRep;