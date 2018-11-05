/*
服务器配置JSON
*/

var fs = require("fs");
var child_process = require("child_process");

ServerConfigType = {
    Type_GateSever: 0,
    Type_GameServer: 1,
    Type_DBServer: 2,
};

function ServerConfig()
{
    // GATE数组
    this.m_GateArray = [];
    // GameServer数组
    this.m_GsArray = [];
}

ServerConfig.prototype.constructor = ServerConfig;

ServerConfig.prototype.LoadConfig =
    function (fileName)
    {
        if (fileName == null)
            return false;

        var file = fs.readFileSync(fileName);
        if (file == null)
            return false;
        var jsonData = JSON.parse(file);
        if (jsonData == null)
            return false;

        this.m_GateArray = jsonData.GateArray;
        this.m_GsArray = jsonData.GsArray;
        
        return true;
    }

//ServerConfig.GatePath = "${__dirname}/Core/RunGate.js";\
ServerConfig.GatePath = "./Core/RunGate.js";

ServerConfig.prototype.RunServer = 
    function (serverType, id, port)
    {
        if (serverType == ServerConfigType.Type_GateSever)
        {
            // 运行Gate服务器
            child_process.fork(ServerConfig.GatePath, [id, port]);
        }
    }

ServerConfig.prototype.RunGates =
    function ()
    {
        console.log("=>Start All Gates");

        if (this.m_GateArray != null)
        {
            for (var i = 0; i < this.m_GateArray.length; ++i)
            {
                var gate = this.m_GateArray[i];
                if (gate != null)
                {
                    // 运行服务器
                    this.RunServer(ServerConfigType.Type_GateSever, gate.id, gate.port);
                }
            }
        }

        console.log("=>End All Gates");
    }

ServerConfig.prototype.RunGss =
    function ()
    {
        console.log("=>Start All Gss");
        console.log("=>End All Gss");
    }

// 根据配置启动服务器组
ServerConfig.prototype.RunConfig =
    function ()
    {
        this.RunGates();
        this.RunGss();
    }

module.exports = ServerConfig;