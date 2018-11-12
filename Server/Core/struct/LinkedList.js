var LinkedListNode = require("./LinkedListNode");


class LinkedList
{
    constructor()
    {
        this.Clear();
    }

    GetCount()
    {
        return this.m_Count;
    }

    GetFirstNode()
    {
        return this.m_First;
    }

    GetLastNode()
    {
        return this.m_Last;
    }

    IsEmpty()
    {
        return this.m_Count <= 0;
    }

    Clear()
    {  
        this.m_First = null;
        this.m_Last = null;
        this.m_Count = 0;
    }

    AddFirstNode(node)
    {
        if (node == null)
            return;
        node.m_Next = this.m_First;
        if (this.m_First != null)
            this.m_First.m_Prev = node;
        this.m_First = node;
        ++this.m_Count;
        if (this.m_Count == 1)
            this.m_Last = this.m_First;
    }

    AddFirstValue(value)
    {
        var node = new LinkedListNode();
        node.m_Value = value;
        AddFirstNode(node);
        return node;
    }

    AddLastNode(node)
    {
        if (node == null)
            return;
        node.m_Prev = this.m_Last;
        if (this.m_Last != null)
            this.m_Last.m_Next = node;
        this.m_Last = node;
        ++this.m_Count;
        if (this.m_Count == 1)
            this.m_First = this.m_Last;
    }

    AddLastValue(value)
    {
        var node = new LinkedListNode();
        node.m_Value = value;
        AddLastNode(node);
        return node;
    }

    AddNode(node, currentNode)
    {
        if (node == null)
            return;

        if (!node.IsReset())
        {
            RemoveNode(node);
        }

        if (currentNode == null)
        {
            this.AddFirstNode(node);
            return;
        }

        var next = currentNode.GetNext();
        currentNode.m_Next = node;
        next.m_Prev = node;
        ++this.m_Count;
    }

    AddValue(value, currentNode)
    {
        var node = new LinkedListNode();
        node.m_Value = value;
        AddNode(node, currentNode);
        return node;
    }

    RemoveNode(node)
    {
        if (node == null)
            return;
        var prev = node.GetPrev();
        var next = node.GetNext();
        if (prev != null)
            prev.m_Next = next;
        if (next != null)
            next.m_Prev = prev;

        /*判斷是否是LAST或者FIRST*/
        if (node == this.m_First)
        {
            this.m_First = next;
        }

        if (node == this.m_Last)
        {
            this.m_Last = prev;
        }
        /*----------------------*/

        node.Reset();
        --this.m_Count;
        if (this.m_Count <= 0)
        {
            this.m_Count = 0;
            this.m_First = null;
            this.m_Last = null;
        }
    }

    RemoveFirstNode()
    {
        var node = this.GetFirstNode();
        RemoveNode(node);
    }

    RemoveLastNode()
    {
        var node = this.GetLastNode();
        RemoveNode(node);
    }

    FindNode(value)
    {
        var node = this.GetFirstNode();
        while (node != null)
        {
            var next = node.GetNext();
            if (node.value == value)
                return node;
            node = next;   
        }
        return null;
    }

    RemoveValue(value)
    {
        var node = this.FindNode(value);
        this.RemoveNode(node);
    }
}

module.exports = LinkedList;