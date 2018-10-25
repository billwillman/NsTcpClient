


// 用户Session数据
function UserSession(socket)
{
    this.m_Socket = socket;
}

UserSession.prototype.constructor = UserSession;


UserSession.prototype.Close = function ()
{
    if (this.m_Socket != null)
    {
        this.m_Socket.destroy();
        this.m_Socket = null;
    }
}


module.exports = UserSession;