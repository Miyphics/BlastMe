using UnityEngine;

public struct SafeInt
{
    private int offset;
    private int value;
    public int Value => value - offset;

    public SafeInt(int value = 0)
    {
        offset = Random.Range(Random.Range(-1000, -1), Random.Range(1, 1001));
        this.value = value + offset;
    }

    public void Dispose()
    {
        offset = 0;
        value = 0;
    }

    public override string ToString()
    {
        return Value.ToString();
    }

    public static SafeInt operator +(SafeInt value1, SafeInt value2)
    {
        return new SafeInt(value1.Value + value2.Value);
    }

    public static SafeInt operator +(SafeInt value1, int value2)
    {
        return new SafeInt(value1.Value + value2);
    }

    public static implicit operator SafeInt(int x)
    {
        return new SafeInt(x);
    }

    public static implicit operator int(SafeInt x)
    {
        return x.Value;
    }
}

public struct SafeUint
{
    private uint offset;
    private uint value;
    public uint Value => value - offset;

    public SafeUint(uint value = 0)
    {
        offset = (uint)Random.Range(1, 1001);
        this.value = value + offset;
    }

    public void Dispose()
    {
        offset = 0;
        value = 0;
    }

    public override string ToString()
    {
        return Value.ToString();
    }

    public static SafeUint operator +(SafeUint value1, SafeUint value2)
    {
        return new SafeUint(value1.Value + value2.Value);
    }

    public static SafeUint operator +(SafeUint value1, uint value2)
    {
        return new SafeUint(value1.Value + value2);
    }

    public static implicit operator SafeUint(uint x)
    {
        return new SafeUint(x);
    }

    public static implicit operator uint(SafeUint x)
    {
        return x.Value;
    }
}

public struct SafeShort
{
    private short offset;
    private short value;
    public short Value => (short)(value - offset);

    public SafeShort(short value = 0)
    {
        offset = (short)Random.Range(Random.Range(-100, -1), Random.Range(1, 101));
        this.value = (short)(value + offset);
    }

    public void Dispose()
    {
        offset = 0;
        value = 0;
    }

    public override string ToString()
    {
        return Value.ToString();
    }

    public static SafeShort operator +(SafeShort value1, SafeShort value2)
    {
        return new SafeShort((short)(value1.Value + value2.Value));
    }

    public static SafeShort operator +(SafeShort value1, int value2)
    {
        return new SafeShort((short)(value1.Value + value2));
    }

    public static SafeShort operator -(SafeShort value1, int value2)
    {
        return value1.Value - value2;
    }

    public static implicit operator SafeShort(int x)
    {
        return new SafeShort((short)x);
    }

    public static implicit operator SafeShort(short x)
    {
        return new SafeShort(x);
    }

    public static implicit operator int(SafeShort x)
    {
        return x.Value;
    }

    public static implicit operator short(SafeShort x)
    {
        return x.Value;
    }
}

public class MAC
{

}
