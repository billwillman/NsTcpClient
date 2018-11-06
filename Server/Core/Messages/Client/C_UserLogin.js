/*
 *用户登录 
*/

var AbstractClientMessage = require("../../AbstractClientMessage");

function C_UserLogin(userId, password)
{
    this.m_userId = userId;
    this.m_password = password;
}

C_UserLogin.prototype = AbstractClientMessage.prototype;
C_UserLogin.prototype.constructor = C_UserLogin;

C_UserLogin.prototype.DoSend =
    function ()
    {
        this.WriteUInt(this.m_userId);
        this.WriteString(this.m_password);
    }

module.exports = C_UserLogin;