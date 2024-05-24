using System.Net;
using System.Net.Sockets;
using System.Buffers;
using System;
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
    private readonly string ip = "127.0.0.1";
    private readonly int port = 55011;
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
    public void SendMessageAsync(byte[] data)
    {
        SendMessageInner(data);
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
            disposed = true;
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
    private void ConnectInner()
    {
        _clientSocket.AddListener(SocketEvent.EVENT_CONNECT, HandleConnect);
        _clientSocket.AddListener(SocketEvent.EVENT_DISCONNECT, HandleDisconnect);
        _clientSocket.RealConnect(_endPoint);
    }
    private void SendMessageInner(byte[] bytes)
    {
        _clientSocket.SendMessageAsync(bytes);
    }
    private void HandleConnect(EventParam eventParam)
    {
        var socket = eventParam.param as Socket;
        CustomLog.Dlog(_TAG, $"HandleConnect connection to endpoint {socket?.RemoteEndPoint}");
    }
    private void HandleDisconnect(EventParam eventParam)
    {
        var socket = eventParam.param as Socket;
        CustomLog.Dlog(_TAG, $"HandleDisconnect connection reset endpoint {socket?.RemoteEndPoint}");
    }
}
