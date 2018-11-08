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

class ServerConfig
{
    constructor()
    {
        // GATE数组
        this.m_GateArray = [];
        // GameServer数组
        this.m_GS = null;
        this.m_DB = null;
    }

    LoadConfig(fileName)
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
        this.m_GS = jsonData.GS;
        this.m_DB = jsonData.DB;
        
        return true;
    }

    RunServer(serverType, id, port)
    {
        if (serverType == ServerConfigType.Type_GateSever)
        {
            // 运行Gate服务器
            child_process.fork(ServerConfig.GatePath, [id, port]);
            //child_process.spawnSync(ServerConfig.GatePath, [id, port]);
        } else if (serverType == ServerConfigType.Type_GameServer)
        {
            child_process.fork(ServerConfig.GsPath, [id, port]);
        } else if (serverType == ServerConfigType.Type_DBServer)
        {
            child_process.fork(ServerConfig.DBPath, [port]);
        }
    }

    RunGates()
    {
        //   console.log("=>Start All Gates");

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

       // console.log("=>End All Gates");
    }

    RunGS()
    {
        if (this.m_GS != null)
        {
            this.RunServer(ServerConfigType.Type_GameServer, 0, this.m_GS.port);
        }
    }  

    RunDB()
    {
        if (this.m_DB != null)
            {
                this.RunServer(ServerConfigType.Type_DBServer, 0, this.m_DB.port);
            }
    }

    // 根据配置启动服务器组
    RunConfig()
    {
        this.RunDB();
        this.RunGS();
        this.RunGates();
    }
}

//ServerConfig.GatePath = "${__dirname}/Core/RunGate.js";\
ServerConfig.GatePath = "./Core/RunGate.js";
ServerConfig.GsPath = "./Core/RunGS.js";
ServerConfig.DBPath = "./Core/RunDB.js";
ServerConfig.RunGatePath = "node.exe " + ServerConfig.GatePath;

module.exports = ServerConfig;