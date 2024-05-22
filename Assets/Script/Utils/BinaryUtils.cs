
using System;

public class BinaryWriter 
{
    public static void WriteIntNoGC(byte[] buffer, int offset, int value)
    {
        if (!EnsureSize(buffer, offset, sizeof(int)))
        {
            throw new IndexOutOfRangeException("buffer over flow");
        }
        unsafe
        {
            fixed (byte* p = &buffer[offset])
            {
                *(int*)p = value;
            }
        }
    }
    public static void WriteUintNoGC(byte[] buffer, int offset, uint value)
    {
        if (!EnsureSize(buffer, offset, sizeof(uint)))
        {
            throw new IndexOutOfRangeException("buffer over flow");
        }
        unsafe
        {
            fixed (byte* p = &buffer[offset])
            {
                *(uint*)p = value;
            }
        }
    }
    public static void WriteShortNoGC(byte[] buffer, int offset, short value)
    {
        if (!EnsureSize(buffer, offset, sizeof(short)))
        {
            throw new IndexOutOfRangeException("buffer over flow");
        }
        unsafe
        {
            fixed (byte* p = &buffer[offset])
            {
                *(short*)p = value;
            }
        }
    }
    public static void WriteCharNoGC(byte[] buffer, int offset, char value)
    {
        if (!EnsureSize(buffer, offset, sizeof(char)))
        {
            throw new IndexOutOfRangeException("buffer over flow");
        }
        unsafe
        {
            fixed (byte* p = &buffer[offset])
            {
                *(char*)p = value;
            }
        }
    }
    public static void WriteLongNoGC(byte[] buffer, int offset, long value)
    {
        if (!EnsureSize(buffer, offset, sizeof(long)))
        {
            throw new IndexOutOfRangeException("buffer over flow");
        }
        unsafe
        {
            fixed (byte* p = &buffer[offset])
            {
                *(long*)p = value;
            }
        }
    }
    public static void WriteUlongNoGC(byte[] buffer, int offset, ulong value)
    {
        if (!EnsureSize(buffer, offset, sizeof(ulong)))
        {
            throw new IndexOutOfRangeException("buffer over flow");
        }
        unsafe
        {
            fixed (byte* p = &buffer[offset])
            {
                *(ulong*)p = value;
            }
        }
    }
    public static void WriteFloatNoGC(byte[] buffer, int offset, float value)
    {
        if (!EnsureSize(buffer, offset, sizeof(float)))
        {
            throw new IndexOutOfRangeException("buffer over flow");
        }
        unsafe
        {
            fixed (byte* p = &buffer[offset])
            {
                *(float*)p = value;
            }
        }
    }
    public static void WriteDoubleNoGC(byte[] buffer, int offset, double value)
    {
        if (!EnsureSize(buffer, offset, sizeof(double)))
        {
            throw new IndexOutOfRangeException("buffer over flow");
        }
        unsafe
        {
            fixed (byte* p = &buffer[offset])
            {
                *(double*)p = value;
            }
        }
    }
    public static void WriteBoolNoGC(byte[] buffer, int offset, bool value)
    {
        if (!EnsureSize(buffer, offset, sizeof(bool)))
        {
            throw new IndexOutOfRangeException("buffer over flow");
        }
        unsafe
        {
            fixed (byte* p = &buffer[offset])
            {
                *(bool*)p = value;
            }
        }
    }
    public static void WriteByteNoGC(byte[] buffer, int offset, byte value)
    {
        if (!EnsureSize(buffer, offset, sizeof(byte)))
        {
            throw new IndexOutOfRangeException("buffer over flow");
        }
        buffer[offset] = value;
    }
    public static void WriteBytesNoGC(byte[] buffer, int offset, byte[] value)
    {
        if (!EnsureSize(buffer, offset, value.Length))
        {
            throw new IndexOutOfRangeException("buffer over flow");
        }
        Array.Copy(value, 0, buffer, offset, value.Length);
    }
    public static bool EnsureSize(byte[] buffer,int offset,int writeSize)
    {
        return buffer.Length - offset >= writeSize;
    }
}
