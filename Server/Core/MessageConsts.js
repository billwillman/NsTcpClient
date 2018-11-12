
var MessageConsts =
{
// 客户端发送协议编号
    ClientMessage:
    {
        // 客户端登录协议
        C_User_Login: 1,
        // 心跳包
        C_Heart: 1000
    },

    // 服务器到客户端的协议编号
    SrvToClientMessage:
    {
        // 登录返回结果
        S_User_loginRet: 1,
        // 心跳包返回
        S_Heart: 1000
    },

    // Gate到GS
    GateToGSMessage : 
    {
        
    },

    FromDBMessage:
    {
        S_User_LoginRet: 1
    },

    // 到DB
    ToDBMessage:
    {
        DB_COMMAND: 1
    },

    GateToLSMessage:
    {
        // 转发：客户端登录
        C_User_Login: 1,
    },

    // 登录服务器到GATE
    LSToGateMessage:
    {

    }
}

module.exports = MessageConsts;