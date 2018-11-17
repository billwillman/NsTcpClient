var LinkedList = require("../struct/LinkedList");
var FrameGroup = require("./FrameGroup");
require("../struct/Utils");

class FrameGroupMgr
{
     constructor(userCount)
     {
        this.m_UserCount = userCount;
        this.m_FrameGroupMap = {};
        this.m_FrameGroupList = new LinkedList();
     }

     AddFrameData(frameIndex, frameData)
     {
         if (frameIndex == null || frameData == null)
            return false;
         var group = this.m_FrameGroupMap[frameIndex];
         if (group == null)
         {
            group = new FrameGroup(frameIndex, this.m_UserCount);
            this.m_FrameGroupMap[frameIndex] = group;
            this.m_FrameGroupList.AddLastNode(group.GetLinkedListNode());
         }
         var ret = group.AddFrameData(frameData);

         if (ret)
         {
            if (this.m_FrameGroupMap[frameIndex] != null)
            {
                delete this.m_FrameGroupMap[frameIndex];
            }

            var node = group.GetLinkedListNode();
            this.m_FrameGroupList.RemoveNode(node);

            this._SendFrameDataGroup(group);
         }

         /* 查看*/
         var firstNode = this.m_FrameGroupList.GetFirstNode();
         if (firstNode != null)
         { 
            var group = firstNode.GetValue();
            var tick = group.GetTick();

            var curTick = _.GetTickTimer();
            if (curTick - tick > FrameGroupMgr.FrameGroupMaxDelayTime)
            {
                var frameIndex = group.GetFrameIndex();
                if (this.m_FrameGroupMap[frameIndex] != null)
                {
                    delete this.m_FrameGroupMap[frameIndex];
                }
                this.m_FrameGroupList.RemoveNode(firstNode);
            } else
            {
                this.m_FrameGroupList.RemoveNode(firstNode);
                this.m_FrameGroupList.AddLastNode(firstNode);
            }
         }


         return ret;
     }

     _SendFrameDataGroup(frameDataGroup)
     {
        // 发送当前帧组
     }
}

FrameGroupMgr.FrameGroupMaxDelayTime = 1;

module.exports = FrameGroupMgr;