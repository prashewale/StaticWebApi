namespace Static.Services.WebSocketClientServices;

// Delegates for events
public delegate void OnConnectHandler();
public delegate void OnCloseHandler();
public delegate void OnErrorHandler(string Message);
public delegate void OnDataHandler(byte[] Data, int Count, string MessageType);

public interface IWebSocketClientService
{
    event OnConnectHandler OnConnect;
    event OnCloseHandler OnClose;
    event OnDataHandler OnData;
    event OnErrorHandler OnError;
    bool IsConnected();
    void Connect(string Url, Dictionary<string, string> headers = null);
    void Send(string Message);
    void Close(bool Abort = false);
}
