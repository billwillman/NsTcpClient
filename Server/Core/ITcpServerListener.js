/*  
侦听器
*/

class ITcpServerListener
{
    constructor() {}

    OnConnectedEvent(socket)
    {}

    OnStartListeningEvent()
    {}

    OnErrorEvent(error)
    {}

    OnSocketEndEvent(socket)
    {}

    OnPacketRead(data)
    {}

    OnTimeOut(socket)
    {}

    OnEndEvent(socket)
    {}

    OnCloseEvent()
    {}
}

module.exports = ITcpServerListener;