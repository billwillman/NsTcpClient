/*
创建GS服务器
*/

var GameServer = require("./GameServer");

function Main()
{
    var arguments = process.argv.splice(2);
    

    var server = GameServer.Create(arguments[0], arguments[1]);

    console.log(" ");
    console.log(arguments);
    if (server != null)
        console.log("=====>>>>启动GS完成=====>>>>");
    else
        console.log("=====>>>>启动GS失敗=====>>>>");
}

Main();