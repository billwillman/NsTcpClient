
function ITcpClientListener()
{

}

ITcpClientListener.prototype.constructor = ITcpClientListener;

ITcpClientListener.prototype.OnAbortEvent = 
    function (tcpClient)
    {}

ITcpClientListener.prototype.OnConnectEvent =
    function(sucess, tcpClient)
    {

    }

module.exports = ITcpClientListener;