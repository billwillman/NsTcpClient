var NetManager = require("./Core/NetManager");
var DefaultPacketHandler = require("./DefaultPacketHandler");

function Main() {

    var inst =  NetManager.GetInstance();
    if (inst == null)
        return;
    var handle = new DefaultPacketHandler();
    inst.SetPacketHandler(handle);
    inst.Listen(1024);
}

// 主循环
Main();