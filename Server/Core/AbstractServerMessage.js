/*
服务器协议
*/

function AbsractServerMessage(buf)
{
    this.m_Buf = buf;
}

AbsractServerMessage.prototype.constructor = AbsractServerMessage;


module.exports = AbsractServerMessage;