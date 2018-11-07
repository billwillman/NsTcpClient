
var MessageConsts =
{
// 客户端发送协议编号
    ClientMessage:
    {
        // 客户端登录协议
        C_User_Login: 1
    },

    // 服务器到客户端的协议编号
    SrvToClientMessage:
    {
        // 登录返回结果
        S_User_loginRet = 1
    },

    // Gate到GS
    GateToGSMessage : 
    {

    },

    // 到DB
    ToDBMessage:
    {

    },

    GateToLSMessage:
    {


    }
}

module.exports = MessageConsts;