using System.Net;
using System.Net.Sockets;
using System.Buffers;
using System;

public  class SocketEvent
{
    public static readonly string EVENT_CONNECT = "EVENT_CONNECT";
    public static readonly string EVENT_DISCONNECT = "EVENT_DISCONNECT";
    public static readonly string EVENT_RECEIVE = "EVENT_RECEIVE";
}
public class ClientSocket:EventDispacth,IDisposable
{
    private readonly string _TAG = "ClientSocket";
    private readonly int MAX_MEMBUFFER_SIZE = 1024 * 8;

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
    private bool disposed;

    public ClientSocket()
    {
        Init();
    }
    /// <summary>
    /// ���ݴ����endpoint�ж�����������
    /// </summary>
    public void RealConnect(EndPoint endPoint)
    {
        _endPoint = endPoint;
        if (_endPoint is IPEndPoint)
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }
        else if (_endPoint is UnixDomainSocketEndPoint)
        {
            _socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Tcp);
        }
        Connect();
    }
    public void StartConnect(string unixEndPoint)
    {
        _socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Tcp);
        Connect();
    }
    public void SendMessageAsync(NetPacket netPacket)
    {
        byte[] data=netPacket.BufferData;
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
        disposed = false;
        CustomLog.Dlog(_TAG, "_receiveBuffer " + MAX_MEMBUFFER_SIZE);
    }
    private void Connect()
    {
        _connectArgs.RemoteEndPoint = _endPoint;
        _connectArgs.Completed += OnConnectCompleted;
        _socket.ConnectAsync(_connectArgs);
    }
    private void OnConnectCompleted(object sender, SocketAsyncEventArgs e)
    {
        if (e.SocketError != SocketError.Success)
        {
            CustomLog.Elog(_TAG, "OnConnectCompleted SocketError");
            return;
        }
        CustomLog.Dlog(_TAG, "OnConnectCompleted �ɹ����ӷ�����");
        DispatchEvent(SocketEvent.EVENT_CONNECT,new EventParam() {sender=this,param=_socket,type="" });
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
        else
        {
            OnReset();
        }
        
    }
    private void OnParse(int length)
    {
        EnsureCapacity(length);
        Array.Copy(_receiveBuffer, 0, _memBuffer, writeIndex, length);
        writeIndex += length;
        while (writeIndex - readIndex >= NetPacket.HEAD_SIZE)
        {
            int packLen = BitConverter.ToInt32(_memBuffer, readIndex);
            int packeID = BitConverter.ToInt32(_memBuffer, readIndex + sizeof(int));
            readIndex += NetPacket.HEAD_SIZE;

            if (packLen > 0 && writeIndex - readIndex >= packLen)
            {
                readIndex += packLen;
                NetPacket packet = new NetPacket(packeID, packLen, true);
                var tempBytes = packet.BufferData;
                Array.Copy(_memBuffer, readIndex, tempBytes, 0, packLen);

                //��������
                DispatchEvent(SocketEvent.EVENT_RECEIVE, new EventParam() { sender = this, param = packet, type = "" });
            }
            else
            {
                readIndex -= NetPacket.HEAD_SIZE;
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
        if (_receiveArgs.LastOperation.Equals(SocketAsyncOperation.Disconnect))
        {
            CustomLog.Dlog(_TAG, "OnReset �Ͽ�����");
            DispatchEvent(SocketEvent.EVENT_DISCONNECT, new EventParam() { sender = this, param = _socket, type = "" });
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposed)
        {
            if (disposing)
            {
                // TODO: �ͷ��й�״̬(�йܶ���)
            }
            _socket.Dispose();
            _connectArgs.Dispose();
            _receiveArgs.Dispose();
            _arrayPool.Return(_receiveBuffer);
            _arrayPool.Return(_memBuffer);
            _sendArgsPool.Clear();
            disposed = true;
        }
    }

    public void Dispose()
    {
        // ��Ҫ���Ĵ˴��롣�뽫���������롰Dispose(bool disposing)��������
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    // TODO: ������Dispose(bool disposing)��ӵ�������ͷ�δ�й���Դ�Ĵ���ʱ������ս���
    ~ClientSocket()
    {
        // ��Ҫ���Ĵ˴��롣�뽫���������롰Dispose(bool disposing)��������
        Dispose(disposing: false);
    }
}
