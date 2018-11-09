

// 字典KEY类
class DictionaryKey
{

    constructor()
    {
        Reset();
    }

    GetValue()
    {
        return this.m_Value;
    }

    Reset()
    {
        this.m_Value = null;
    }

    IsReset()
    {
        return this.m_Value == null;
    }

    HashCode()
    {
        var str = ToString();
        if (str != null)
        {
            return this.StrToHash(str);
        }
        return this;
    }

    ToString()
    {
        return null;
    }

    // 字符串HASH
    static StrToHash(str)
    {
        if (str == null)
            throw new Exception();
        var hash = DictionaryKey._cHash;
        for (i = 0; i < str.length; i++) {
            char = str.charCodeAt(i);
            hash = ((hash << 5) + hash) + char; /* hash * 33 + c */
        }
        return hash;
    }

    static InitHashCode()
    {
        return DictionaryKey._cHash;
    }

    /*
    static HashCode(hashCode, value)
    {
        if (hashCode == null)
            hashCode = this.InitHashCode();
        var type = typeof(value);
        if (type == "number")
        {
            // 数字
        } else if (type == "boolean")
        {
            // bool
        } else if (type == "undefined")
        {
            // 未定义
        }
        return hashCode;
    }*/

}

DictionaryKey._cHash = 5381;

module.export = DictionaryKey;