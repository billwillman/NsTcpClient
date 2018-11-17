/*
Frame组，所有用户当前帧
*/

var LinkedList = require("../struct/LinkedList");
var LinkedListNode = require("../struct/LinkedListNode");
require("../struct/Utils");

class FrameGroup
{
    // 用户数量
    constructor(frameIndex, userCount)
    {
        this.m_UserCount = userCount;
        this.m_FrameIndex = frameIndex;
        this.m_UserFrameList = new LinkedList();
        this.m_UserFrameMap = {};
        this.m_Node = new LinkedListNode(this);
        this.UpdateUsedTick();
    }

    GetFrameIndex()
    {
        return this.m_FrameIndex;
    }

    UpdateUsedTick()
    {
        this.m_Tick = _.GetTickTimer();
    }

    GetTick()
    {
        return this.m_Tick;
    }

    GetLinkedListNode()
    {
        return this.m_Node;
    }

    // 是否准备好
    IsReady()
    {
        return this.m_UserFrameList.GetCount() >= this.m_UserCount;
    }

    AddFrameData(frameData)
    {
        if (frameData == null)
            return false;
        var userId = frameData.GetUserId();
        if (userId == null)
            return false;
        
        var oldData = this.m_UserFrameMap[userId];
        if (oldData == frameData)
            return true;
        var isUpdateData = oldData != null;
        var node = oldData.GetLinkedListNode();
        this.m_UserFrameList.RemoveNode(node);
        oldData = null;

        node = frameData.GetLinkedListNode();
        this.m_UserFrameMap[userId] = node;
        this.m_UserFrameList.AddLastNode(node);

        // 更新TICK, 只有是真的新加数据才可以UPDATE，防止一个人用外挂攻击，乱跳帧
        if (!isUpdateData)
            this.UpdateUsedTick();

        return this.IsReady();
    }
}

module.exports = FrameGroup;