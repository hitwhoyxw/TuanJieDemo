using Google.Protobuf;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;

public static class ProtobufExtention 
{
    /// <summary>
    /// 返回的byte数组需要return
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public static byte[] ToByteArrayOptimised(this IMessage message)
    {
        ProtoPreconditions.CheckNotNull(message, "message");
        int size= message.CalculateSize();
        byte[] array=ArrayPool<byte>.Shared.Rent(size);
        //byte[] array = new byte[message.CalculateSize()];
        CodedOutputStream codedOutputStream = new CodedOutputStream(array,0,size);
        message.WriteTo(codedOutputStream);
        codedOutputStream.CheckNoSpaceLeft();
        return array;
    }
}
