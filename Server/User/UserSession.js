


// 用户Session数据
function UserSession(socket)
{
    this.m_Socket = socket;
}

UserSession.prototype.constructor = UserSession;

// 关闭类型
UserSession.prototype.EndType =
{
    "Server_End": 0
}


UserSession.prototype.Close = function ()
{
    if (this.m_Socket != null)
    {
        this.m_Socket.end(UserSession.Server_End);
        this.m_Socket = null;
    }
}


module.exports = UserSession;