var TcpServer = require("./Core/TcpServer");

function Main() {
    var server = new TcpServer(1024);
    if (server != null)
    {
        // 开始侦听
        server.Accept();
    }
}

// 主循环
Main();