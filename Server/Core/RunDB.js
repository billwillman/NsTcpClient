var DBServer = require("./DBServer");

function Main()
{
    var arguments = process.argv.splice(2);

    if (arguments[0] == null)
        arguments[0] = 3024;

    server = DBServer.Create(arguments[0]);
    console.log(" ");
    console.log(arguments);
    if (server != null)
        console.log("=====>>>>启动DB完成=====>>>>");
    else
        console.log("=====>>>>启动DB失敗=====>>>>");
}

Main();