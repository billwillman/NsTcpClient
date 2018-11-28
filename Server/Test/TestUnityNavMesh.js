var NetManager = require("../Core/NetManager");
var DefaultPacketHandler = require("../Core/DefaultPacketHandler");
var edge = require("edge");

class TestUnityNavMeshServer extends NetManager
{
    constructor()
    {
        super();
        this._LoadUnityDll();
    }

    _LoadUnityDll()
    {

    }
}

function Main()
{
    var server = new TestUnityNavMeshServer();
    server.SetPacketHandlerClass(DefaultPacketHandler);
    server.Listen(8000);
}

Main();