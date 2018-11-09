
class LinkedListNode
{
    constructor(value)
    {
        this.m_Prev = null;
        this.m_Next = null;
        this.m_Value = value;
    }

    GetValue()
    {
        return this.m_Value;
    }

    GetPrev()
    {
        return this.m_Prev;
    }

    GetNext()
    {
        return this.m_Next;
    }

    Reset()
    {
        this.m_Prev = null;
        this.m_Next = null;
        this.m_Value = null;
    }

    IsReset()
    {
        return this.m_Next == null && this.m_Prev == null;
    }
}