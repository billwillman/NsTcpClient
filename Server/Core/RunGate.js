
var GateServer = require("./GateServer");

var server = null;

// 创建GATE服务器
function Main()
{
    // 获得命令行参数 编号和port
    var arguments = process.argv.splice(2);
    if (arguments[0] == null)
        arguments[0] = 0;
    if (arguments[1] == null)
        arguments[1] = 1024;

    server = GateServer.Create(arguments[0], arguments[1]);
    console.log(" ");
    console.log(arguments);
    if (server != null)
        console.log("=====>>>>启动GATE完成=====>>>>");
    else
        console.log("=====>>>>启动GATE失敗=====>>>>");
}

Main();