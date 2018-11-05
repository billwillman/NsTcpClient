var ServerConfig = require("./ServerConfig");
var serverConfig = null;

function Main()
{
    serverConfig = new ServerConfig();
    serverConfig.LoadConfig("./Config/server.json");
    serverConfig.RunConfig();
}

Main();