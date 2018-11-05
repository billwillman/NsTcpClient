
var GateServer = require("./GateServer");

var server = null;

// 创建GATE服务器
function Main()
{
    // 获得命令行参数 编号和port
    console.log("====>>>>启动GATE服务器====>>>>");
    var arguments = process.argv.splice(2);
    console.log(arguments);

    server = GateServer.Create(arguments[0], arguments[1]);
    if (server != null)
        console.log("=====>>>>启动GATE完成=====>>>>");
    else
        console.log("=====>>>>启动GATE失敗=====>>>>");
}

Main();