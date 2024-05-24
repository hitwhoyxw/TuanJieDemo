using System.Net;
using System.Net.Sockets;
using System.Buffers;
using System;
public enum NetworkType{
    Local,
    Internet
}
/// <summary>
/// 客户端网络数据收发
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
                // TODO: 释放托管状态(托管对象)
            }
            // TODO: 释放未托管的资源(未托管的对象)并重写终结器
            // TODO: 将大型字段设置为 null
            disposed = true;
        }
    }

    // TODO: 仅当“Dispose(bool disposing)”拥有用于释放未托管资源的代码时才替代终结器
    ~NetClient()
    {
        // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
        Dispose(disposing: false);
    }

    void IDisposable.Dispose()
    {
        // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
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
