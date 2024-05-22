using System;
using System.Buffers;

public class NetPacket
{
    public static readonly int HEAD_SIZE = 2 * sizeof(int);
    public int Index { get; private set; }
    public byte[] BufferData { get; set; }
    public int Length { get; set; }
    public int MessageID { get; set; }

    private int AvailableLen { get; set; }

    private ArrayPool<byte> _arrayPool = ArrayPool<byte>.Shared;
    /// <summary>
    /// for write
    /// </summary>
    /// <param name="bytes"></param>
    /// <param name="messageID"></param>
    public NetPacket(int id, int dataLen, bool isRead)
    {
        Index = 0;
        MessageID = id;

        int allocSize = isRead ? dataLen : dataLen + 2 * sizeof(int);
        AvailableLen = allocSize;

        BufferData = _arrayPool.Rent(allocSize);

        if (isRead)
        {
            return;
        }
        WriteInt(dataLen);
        WriteInt(id);
    }
    public void Clear()
    {
        Index = 0;
        MessageID = 0;
        Length = 0;
        AvailableLen = 0;
        if (BufferData != null)
        {
            _arrayPool.Return(BufferData);
            BufferData = null;
        }
    }
    private void WriteInt(int intMsg)
    {
        BinaryWriter.WriteIntNoGC(BufferData, Index, intMsg);
        Index += sizeof(int);
    }
}