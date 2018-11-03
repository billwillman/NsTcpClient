/*
  DB服务器
*/

var ITcpServerListener = require("./ITcpServerListener");

function DBServer()
{}

DBServer.prototype = ITcpServerListener.prototype;
DBServer.prototype.constructor = DBServer;

module.exports = DBServer;