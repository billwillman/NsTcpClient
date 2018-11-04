/*
服务器配置JSON
*/

var fs = require("fs");
var child_process = require("child_process");

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

ServerConfig.prototype.RunGates =
    function ()
    {

    }

ServerConfig.prototype.RunGss =
    function ()
    {

    }

// 根据配置启动服务器组
ServerConfig.prototype.RunConfig =
    function ()
    {
        this.RunGates();
        this.RunGss();
    }

module.exports = ServerConfig;