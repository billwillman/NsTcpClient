/*
服务器配置JSON
*/

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
        return true;
    }

module.exports = ServerConfig;