using System.Net;
using System.Net.Sockets;
using System.Buffers;
using System;
/// <summary>
/// �ͻ������������շ�
/// </summary>
public class NetClient : Singleton<NetClient>, IDisposable
{
    private readonly string _TAG = "NetClient";
    private readonly int MAX_MEMBUFFER_SIZE = 1024 * 8;
    private readonly string _socketPath = "/tmp/UnitySocket";

    private Socket _socket;
    private EndPoint _endPoint;
    private byte[] _receiveBuffer;
    private byte[] _memBuffer;
    private int readIndex = 0;
    private int writeIndex = 0;
    private SocketAsyncEventArgs _connectArgs;
    private SocketAsyncEventArgs _receiveArgs;
    private ConcurrentObjectPool<SocketAsyncEventArgs> _sendArgsPool;
    private ArrayPool<byte> _arrayPool = ArrayPool<byte>.Shared;
    private bool disposedValue;

    public NetClient()
    {
        Init();
    }
    public void StartConnect(string ip, int port)
    {
        _endPoint = new IPEndPoint(IPAddress.Parse(ip), port);
        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        Connect();
    }
    public void StartConnect(string unixEndPoint)
    {
        _endPoint = new UnixDomainSocketEndPoint(_socketPath);
        _socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Tcp);
        Connect();
    }
    public void SendMessageAsync(byte[] data)
    {
        var sendArgs = _sendArgsPool.Get();
        sendArgs.SetBuffer(data, 0, data.Length);
        sendArgs.Completed += (sender, e) =>
        {
            _sendArgsPool.Recycle(e);
        };
        _socket.SendAsync(sendArgs);
    }
    private void Init()
    {
        _connectArgs = new SocketAsyncEventArgs();
        _receiveArgs = new SocketAsyncEventArgs();
        _sendArgsPool = new ConcurrentObjectPool<SocketAsyncEventArgs>(10, () => { return new SocketAsyncEventArgs(); }, false);
        _receiveBuffer = _arrayPool.Rent(MAX_MEMBUFFER_SIZE);
        _memBuffer = _arrayPool.Rent(MAX_MEMBUFFER_SIZE);
        CustomLog.Dlog(_TAG, "_receiveBuffer " + MAX_MEMBUFFER_SIZE);
    }
    private void Connect()
    {
        _connectArgs.RemoteEndPoint = _endPoint;
        _socket.ConnectAsync(_connectArgs);
    }
    private void OnConnectCompleted(object sender, SocketAsyncEventArgs e)
    {
        if (e.SocketError != SocketError.Success)
        {
            CustomLog.Elog(_TAG, "OnConnectCompleted SocketError");
            return;
        }
        StartReceive();
    }
    private void StartReceive()
    {
        _receiveArgs = new SocketAsyncEventArgs();
        _receiveArgs.SetBuffer(_receiveBuffer, 0, MAX_MEMBUFFER_SIZE);

        var willRaiseEvent = _socket.ReceiveAsync(_receiveArgs);
        if (!willRaiseEvent)
        {
            ProcessReceive(_receiveArgs);
        }
    }

    private void ProcessReceive(SocketAsyncEventArgs receiveArgs)
    {
        if (receiveArgs.SocketError != SocketError.Success)
        {
            return;
        }
        if (receiveArgs.SocketError == SocketError.ConnectionAborted || receiveArgs.SocketError == SocketError.ConnectionReset)
        {
            OnReset();
        }
        int length = receiveArgs.BytesTransferred;
        if (length > 0)
        {
            OnParse(length);
            _socket.ReceiveAsync(receiveArgs);
        }
        OnReset();
    }
    private void OnParse(int length)
    {
        EnsureCapacity(length);
        Array.Copy(_receiveBuffer, 0, _memBuffer, writeIndex, length);
        writeIndex += length;
        while (writeIndex - readIndex >= NetPacket.HEAD_SIZE)
        {
            int packLen= BitConverter.ToInt32(_memBuffer, readIndex);
            int packeID = BitConverter.ToInt32(_memBuffer, readIndex + sizeof(int));
            readIndex += NetPacket.HEAD_SIZE;

            if (packLen > 0&& writeIndex - readIndex >= packLen)
            {
                readIndex += packLen;
                NetPacket packet = new NetPacket(packeID, packLen, true);
                var tempBytes=packet.BufferData;
                Array.Copy(_memBuffer, readIndex, tempBytes, 0, packLen);

                //��������
            }
            else
            {
                readIndex-= NetPacket.HEAD_SIZE;
            }
        }
    }
    private void EnsureCapacity(int required)
    {
        if (_memBuffer == null)
        {
            _memBuffer = _arrayPool.Rent(MAX_MEMBUFFER_SIZE);
            writeIndex = 0;
            readIndex = 0;
        }
        //ʣ��ռ乻��ֱ��д
        else if (_memBuffer.Length - writeIndex >= required)
        {
            return;
        }
        //ʣ��ռ䲻��
        else
        {
            //����û�õ�+��û����
            int availableBytes = (_memBuffer.Length - 1 - writeIndex) + readIndex;
            if (availableBytes >= required)
            {
                Array.Copy(_memBuffer, readIndex, _memBuffer, 0, writeIndex - readIndex);
            }

            //�����£�2������
            else
            {
                byte[] temp = _arrayPool.Rent(_memBuffer.Length * 2);
                Array.Copy(_memBuffer, readIndex, temp, 0, writeIndex - readIndex);

                _arrayPool.Return(_memBuffer, true);
                _memBuffer = temp;
            }
            writeIndex -= readIndex;
            readIndex = 0;
        }
    }
    /// <summary>
    /// �Ͽ���Ĳ���
    /// </summary>
    private void OnReset()
    {

    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // TODO: �ͷ��й�״̬(�йܶ���)
                _socket.Dispose();
                _connectArgs.Dispose();
                _receiveArgs.Dispose();
            }
            // TODO: �ͷ�δ�йܵ���Դ(δ�йܵĶ���)����д�ս���
            // TODO: �������ֶ�����Ϊ null
            _sendArgsPool = null;
            _arrayPool.Return(_receiveBuffer);
            _receiveBuffer = null;
            disposedValue = true;
        }
    }

    // TODO: ������Dispose(bool disposing)��ӵ�������ͷ�δ�й���Դ�Ĵ���ʱ������ս���
    ~NetClient()
    {
        // ��Ҫ���Ĵ˴��롣�뽫���������롰Dispose(bool disposing)��������
        Dispose(disposing: false);
    }

    void IDisposable.Dispose()
    {
        // ��Ҫ���Ĵ˴��롣�뽫���������롰Dispose(bool disposing)��������
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
