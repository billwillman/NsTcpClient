/*
创建GS服务器
*/

var GameServer = require("./GameServer");

function Main()
{
    var arguments = process.argv.splice(2);
    
    if (arguments[0] == null)
        arguments[0] = 0;
    if (arguments[1] == null)
        arguments[1] = 2024;
    if (arguments[2] == null)
        arguments[2] = "127.0.0.1";
    if (arguments[3] == null)
        arguments[3] = 3024;

    var server = GameServer.Create(arguments[0], arguments[1], arguments[2], arguments[3]);

    console.log(" ");
    console.log(arguments);
    if (server != null)
        console.log("=====>>>>启动GS完成=====>>>>");
    else
        console.log("=====>>>>启动GS失敗=====>>>>");
}

Main();