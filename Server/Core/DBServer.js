/*
  DB服务器
*/

var NetManager = require("./NetManager");

function DBServer()
{}

DBServer.prototype = NetManager.prototype;
DBServer.prototype.constructor = DBServer;

module.exports = DBServer;