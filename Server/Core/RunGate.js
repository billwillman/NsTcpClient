
// 创建GATE服务器
function Main()
{
    // 获得命令行参数 编号和port
    console.log("====>>>>启动GATE服务器====>>>>");
    var arguments = process.argv.splice(2);
    console.log(arguments);
    console.log("=====>>>>启动GATE完成=====>>>>");
}

Main();