/*
 * 心跳包 
*/

var AbstractClientMessage = require("../../AbstractClientMessage");


function C_HeartMessage()
{}

C_HeartMessage.prototype = AbstractClientMessage.prototype;
C_HeartMessage.prototype.constructor = C_HeartMessage;

module.exports = C_HeartMessage;