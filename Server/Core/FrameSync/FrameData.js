/*
    FrameData一针的数据
*/

var LinkedListNode = require("../struct/LinkedListNode");

FrameDataType =
{
    // 空帧
    ZeroFrame : 0,
    // 关键帧
    KeyFrame = 1
}

// 一个用户的数据
class FrameData
{
    // userId: 用户ID
    // frameId: 帧编号
    // keyControl: 按键
    constructor(userId, keyControl)
    {
        this.m_UserId = userId;
        this.m_KeyControl = keyControl;
        this.m_Node = new LinkedListNode(this);
    }

    ToMessage(clientMsg)
    {
        if (clientMsg == null)
            return;
        var frameType = this.m_KeyControl == 0? FrameDataType.ZeroFrame: FrameDataType.KeyFrame;
        clientMsg.WriteByte(frameType);
        clientMsg.WriteUInt(this.m_UserId);
        if (frameType != FrameDataType.ZeroFrame)
            clientMsg.WriteInt(this.m_KeyControl);
    }

    GetLinkedListNode()
    {
        return this.m_Node;
    }

    GetUserId()
    {
        return this.m_UserId;
    }

    GetKeyControl()
    {
        return this.m_KeyControl;
    }
}

module.exports = FrameData;