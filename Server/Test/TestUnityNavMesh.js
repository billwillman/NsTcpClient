var NetManager = require("../Core/NetManager");
var DefaultPacketHandler = require("../Core/DefaultPacketHandler");
var recast = require("../recast/build/Release/recast")

class TestUnityNavMeshServer extends NetManager
{
    constructor()
    {
        super();
        this.m_Recast = new recast.recastObj()
    }

    // 加载地图
    LoadMapObj(fileName)
    {
        if (this.m_Recast == null)
            return false;
        return this.m_Recast.LoadMapObj(fileName);
    }
}

function Main()
{
    var server = new TestUnityNavMeshServer();
    server.SetPacketHandlerClass(DefaultPacketHandler);
    server.Listen(8000);
}

Main();