
var LinkedList = require("./LinkedList");
var LinkedListNode = require("./LinkedListNode");
require("ObjEqual");

class Dictionary
{
    constructor()
    {
        // MAP存放的是LINKEDLIST
        this.Clear();
    }

    GetCount()
    {
        return this.m_Count;
    }

    Clear()
    {
        this.m_Map = null;
        this.m_Count = 0;
    }

    Add(key, value)
    {
        if (key == null)
            return false;
        if (this.m_Map == null)
        {
            this.m_Map = {};

            var hashCode;
            if (key.HashCode != null)
                hashCode = key.HashCode();
            else
                hashCode = key;
            var list = new LinkedList();
            this.m_Map[hashCode] = list;

            var target = {};
            // Clone一个KEY
            target.key = Object.assign({}, key);
            target.value = value;
            list.AddLastValue(target);

            ++this.m_Count;

            return true;
        } else
        {
            var hashCode;
            if (key.HashCode != null)
                hashCode = key.HashCode();
            else
                hashCode = key;

            var list = this.m_Map[hashCode];
            var node = list.GetFirstNode();
            while (node != null)
            {
                var next = node.GetNext();

                var target = node.GetValue();
                if (target.key == key)
                {
                    target.value = value;
                    return true;
                }

                if (_.isEqual(target.key, key))
                {
                    target.value = value;
                    return true;
                }

                node = next;
            }

            var target = {};
            // Clone一个KEY
            target.key = Object.assign({}, key);
            target.value = value;
            list.AddLastValue(target);
            ++this.m_Count;

            return true;
        }
    }

    GetValue(key)
    {
        if (key == null || this.m_Map == null)
            return null;
        
        var hashCode;
        if (key.HashCode != null)
            hashCode = key.HashCode();
        else
            hashCode = key;

        var list = this.m_Map[hashCode];
        if (list == null)
            return null;
        
        var node = list.GetFirstNode();
        while (node != null)
        {
            var next = node.GetNext();
            var target = node.value;
            if (target.key == key)
                return target.value;

            if (_.isEqual(target.key, key))
                return target.value;

            node = next;
        }

        return null;
    }

    IsContains(key)
    {
        var value = this.GetValue(key);
        return value != null;
    }

    RemoveKey(key)
    {
        if (key == null || this.m_Map == null)
            return false;

        var hashCode;
        if (key.HashCode != null)
            hashCode = key.HashCode();
        else
            hashCode = key;

        var list = this.m_Map[hashCode];
        if (list == null)
            return false;

        var node = list.GetFirstNode();
        while (node != null)
        {
            var next = node.GetNext();
            var target = node.GetValue();
            if (target.key == key || _.isEqual(target.key, key))
            {
                list.RemoveNode(node);
                --this.m_Count;
                return true;
            }
            node = next;
        }
        
        
        return false;
    }

    ForEach(callBack)
    {
        if (callBack == null || this.m_Map == null)
            return;

        var type = typeof(callBack);
        if (type != "function")
            return;
        
        for (var list in this.m_Map)
        {
            var node = list.GetFirstNode();
            while (node != null)
            {
                var next = node.GetNext();

                var target = node.GetValue();
                callBack(target.key, target.value);

                node = next;
            }
        }
    }
}

module.exports = Dictionary;