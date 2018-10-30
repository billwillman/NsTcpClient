/*  
侦听器
*/

function ITcpServerListener()
{ }

ITcpServerListener.prototype.constructor = ITcpServerListener;


ITcpServerListener.prototype.OnConnectedEvent =
    function (socket)
    {}

ITcpServerListener.prototype.OnStartListeningEvent =
    Function ()
    {}

ITcpServerListener.prototype.OnErrorEvent = 
    function (error)
    {}


ITcpServerListener.prototype.OnSocketEndEvent =
        function (socket)
        {};

ITcpServerListener.prototype.OnPacketRead =
    function (data)
    {}

ITcpServerListener.prototype.OnEndEvent =
    function (socket)
    {}

ITcpServerListener.prototype.OnCloseEvent =
    function()
    {}

module.exports = ITcpServerListener;