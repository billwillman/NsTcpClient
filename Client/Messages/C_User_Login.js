/*
客户端登录协议
*/
var AbstractClientMessage = require("../../Server/Core/AbstractClientMessage");

function C_User_Login(user, password)
{  
    this.user = user;
    this.password = password;
}

C_User_Login.prototype = AbstractClientMessage.prototype;
C_User_Login.prototype.constructor = C_User_Login;
C_User_Login.prototype.DoSend =
    function ()
    {
        this.WriteString(this.user);
        this.WriteString(this.password);
    }

module.exports = C_User_Login;

