
class IUdpServerListener
{
    constructor()
    {}

    OnErrorEvent(error)
    {}
    
    OnStartListeningEvent()
    {}

    OnMessage(msg, info)
    {}

    OnMessagePacket(packet, info)
    {}
}