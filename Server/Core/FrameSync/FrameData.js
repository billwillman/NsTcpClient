/*
    FrameData一针的数据
*/

var LinkedListNode = require("../struct/LinkedListNode");

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
        clientMsg.WriteUInt(this.m_UserId);
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