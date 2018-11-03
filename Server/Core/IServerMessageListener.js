/*
  网络协议处理
*/

function IServerMessagListener()
{}

IServerMessagListener.prototype.constructor = IServerMessagListener;

IServerMessagListener.prototype.OnMessage = function (packet, clientSocket, netMgr)
{}

module.exports = IServerMessagListener;