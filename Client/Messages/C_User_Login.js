/*
客户端登录协议
*/
var AbstractClientMessage = require("../../Server/Core/AbstractClientMessage");

class C_User_Login extends AbstractClientMessage
{
    constructor(user, password)
    {
        super();
        this.user = user;
        this.password = password;
    }

    DoSend()
    {  
        this.WriteString(this.user);
        this.WriteString(this.password);
    }
}

module.exports = C_User_Login;

