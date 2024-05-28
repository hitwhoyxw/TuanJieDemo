using System.Net;
using System.Net.Sockets;
using System.Buffers;
using System;
using System.Threading.Tasks;
using Mytest;
using Google.Protobuf;
public enum NetworkType{
    Local,
    Internet
}
/// <summary>
/// �ͻ������������շ�
/// </summary>
public class NetClient : Singleton<NetClient>, IDisposable
{
    private readonly string _TAG = "NetClient";
    private readonly string UNIX_SOCKET_PATH = "/tmp/UnitySocket";
    private ClientSocket _clientSocket;
    private EndPoint _endPoint;
    private bool disposed;
    
    public NetClient()
    {
        disposed = false;
    }
    public void StartConnect(string ip, int port)
    {
        _endPoint = new IPEndPoint(IPAddress.Parse(ip), port);
        _clientSocket = new ClientSocket();
        ConnectInner();
    }
    public void StartConnect()
    {
        //_endPoint = new UnixDomainSocketEndPoint(UNIX_SOCKET_PATH);
        _endPoint = new IPEndPoint(IPAddress.Loopback, 55011);
        _clientSocket = new ClientSocket();
        ConnectInner();
    }
    public void SendMessageAsync(MessageID messageID,IMessage message)
    {
        SendMessageInner(messageID, message);
    }
    protected virtual void Dispose(bool disposing)
    {
        if (!disposed)
        {
            if (disposing)
            {
                // TODO: �ͷ��й�״̬(�йܶ���)
            }
            // TODO: �ͷ�δ�йܵ���Դ(δ�йܵĶ���)����д�ս���
            // TODO: �������ֶ�����Ϊ null
            _clientSocket.Dispose();
            disposed = true;
        }
    }

    // TODO: ������Dispose(bool disposing)��ӵ�������ͷ�δ�й���Դ�Ĵ���ʱ������ս���
    ~NetClient()
    {
        Dispose(disposing: false);
    }

    void IDisposable.Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
    private void ConnectInner()
    {
        _clientSocket.AddListener(SocketEvent.EVENT_CONNECT, HandleConnect);
        _clientSocket.AddListener(SocketEvent.EVENT_DISCONNECT, HandleDisconnect);
        _clientSocket.RealConnect(_endPoint);
    }
    private void SendMessageInner(MessageID messageID,IMessage message)
    {
        var bytes = message.ToByteArray();
        NetPacket netPacket = new NetPacket((int)messageID, bytes.Length, false);
        netPacket.WriteBytes(bytes, bytes.Length,0);
        _clientSocket.SendMessageAsync(netPacket);
    }
    private void HandleConnect(EventParam eventParam)
    {
        var socket = eventParam.param as Socket;
        Task.Run( () =>
        {
            Task.Delay(1000);
            SendMessageAsync(MessageID.HeartBeat, new RequestToken() {Token=socket.LocalEndPoint.ToString()});
        });
        CustomLog.Dlog(_TAG, $"HandleConnect connection to endpoint {socket?.RemoteEndPoint}");
    }
    private void HandleDisconnect(EventParam eventParam)
    {
        var socket = eventParam.param as Socket;
        CustomLog.Dlog(_TAG, $"HandleDisconnect connection reset endpoint {socket?.RemoteEndPoint}");
    }
}
