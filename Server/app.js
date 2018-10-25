var NetManager = require("./Core/NetManager");

function Main() {

    var inst =  NetManager.GetInstance();
    if (inst == null)
        return;
    inst.Listen(1024);
}

// 主循环
Main();