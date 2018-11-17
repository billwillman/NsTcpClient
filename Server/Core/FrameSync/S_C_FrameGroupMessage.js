var AbstractClientMessage = require("../AbstractClientMessage");

class S_C_FrameGroupMessage extends AbstractClientMessage
{
    constructor(frameGroup)
    {
        super();
        this.m_FrameGroup = frameGroup;
    }

    DoSend()
    {
        // 发送数据
        this.m_FrameGroup.ToMessage(this);
    }
}

module.exports = S_C_FrameGroupMessage;