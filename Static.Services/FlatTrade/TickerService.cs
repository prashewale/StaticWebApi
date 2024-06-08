using Microsoft.VisualBasic;
using Static.Services.Models;
using Static.Services.WebSocketClientServices;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace Static.Services.FlatTrade;

public class TickerService<T> where T : ITick
{

    // If set to true will print extra debug information
    private bool DebugMode { get; set; }

    // Root domain for ticker. Can be changed with Root parameter in the constructor.
    public string RootUrl { get; set; }
    public string AccessToken { get; set; }

    // Configurations to create ticker connection
    public string SocketUrl { get; set; }
    public bool ShouldReconnect { get; set; }
    public int RetryInterval { get; set; } = 5;
    public int MaxRetryCount { get; set; } = 50;
    private int _retryCount = 0;

    public Dictionary<string, string> Headers { get; set; } = [];

    // A watchdog timer for monitoring the connection of ticker.
    private readonly System.Timers.Timer _timer;
    private int _timerTick = 5;

    // Instance of WebSocket class that wraps .Net version
    private IWebSocketClientService _ws;

    // Dictionary that keeps instrument_token -> mode data
    private Dictionary<uint, string> _subscribedTokens = [];

    // Delegates for callbacks

    /// <summary>
    /// Delegate for OnConnect event
    /// </summary>
    public delegate void OnConnectHandler();

    /// <summary>
    /// Delegate for OnClose event
    /// </summary>
    public delegate void OnCloseHandler();

    /// <summary>
    /// Delegate for OnTick event
    /// </summary>
    /// <param name="TickData">Tick data</param>
    public delegate void OnTickHandler(ITick TickData);

    ///// <summary>
    ///// Delegate for OnOrderUpdate event
    ///// </summary>
    ///// <param name="OrderData">Order data</param>
    //public delegate void OnOrderUpdateHandler(Order OrderData);

    /// <summary>
    /// Delegate for OnError event
    /// </summary>
    /// <param name="Message">Error message</param>
    public delegate void OnErrorHandler(string Message);

    /// <summary>
    /// Delegate for OnReconnect event
    /// </summary>
    public delegate void OnReconnectHandler();

    /// <summary>
    /// Delegate for OnNoReconnect event
    /// </summary>
    public delegate void OnNoReconnectHandler();

    // Events that can be subscribed
    /// <summary>
    /// Event triggered when ticker is connected
    /// </summary>
    public event OnConnectHandler OnConnect;

    /// <summary>
    /// Event triggered when ticker is disconnected
    /// </summary>
    public event OnCloseHandler OnClose;

    /// <summary>
    /// Event triggered when ticker receives a tick
    /// </summary>
    public event OnTickHandler OnTick;

    ///// <summary>
    ///// Event triggered when ticker receives an order update
    ///// </summary>
    //public event OnOrderUpdateHandler OnOrderUpdate;

    /// <summary>
    /// Event triggered when ticker encounters an error
    /// </summary>
    public event OnErrorHandler OnError;

    /// <summary>
    /// Event triggered when ticker is reconnected
    /// </summary>
    public event OnReconnectHandler OnReconnect;

    /// <summary>
    /// Event triggered when ticker is not reconnecting after failure
    /// </summary>
    public event OnNoReconnectHandler OnNoReconnect;

    /// <summary>
    /// Initialize websocket client instance.
    /// </summary>
    /// <param name="APIKey">API key issued to you</param>
    /// <param name="UserID">Zerodha client id of the authenticated user</param>
    /// <param name="AccessToken">Token obtained after the login flow in 
    /// exchange for the `request_token`.Pre-login, this will default to None,
    /// but once you have obtained it, you should
    /// persist it in a database or session to pass
    /// to the Kite Connect class initialisation for subsequent requests.</param>
    /// <param name="Root">Websocket API end point root. Unless you explicitly 
    /// want to send API requests to a non-default endpoint, this can be ignored.</param>
    /// <param name="Reconnect">Enables WebSocket autreconnect in case of network failure/disconnection.</param>
    /// <param name="ReconnectInterval">Interval (in seconds) between auto reconnection attemptes. Defaults to 5 seconds.</param>
    /// <param name="ReconnectTries">Maximum number reconnection attempts. Defaults to 50 attempts.</param>
    public TickerService(IWebSocketClientService ws)
    {
        _ws = ws;

        _ws.OnConnect += _onConnect;
        _ws.OnData += _onData;
        _ws.OnClose += _onClose;
        _ws.OnError += _onError;

        // initializing  watchdog timer
        _timer = new System.Timers.Timer();
        _timer.Elapsed += _onTimerTick;
        _timer.Interval = 1000; // checks connection every second
    }

    private void _onError(string Message)
    {
        // pipe the error message from ticker to the events
        OnError?.Invoke(Message);
    }

    private void _onClose()
    {
        // stop the timer while normally closing the connection
        _timer.Stop();
        OnClose?.Invoke();
    }

    /// <summary>
    /// Reads 2 byte short int from byte stream
    /// </summary>
    private static ushort ReadShort(byte[] b, ref int offset)
    {
        ushort data = (ushort)(b[offset + 1] + (b[offset] << 8));
        offset += 2;
        return data;
    }

    /// <summary>
    /// Reads 4 byte int32 from byte stream
    /// </summary>
    private static uint ReadInt(byte[] b, ref int offset)
    {
        uint data = BitConverter.ToUInt32([b[offset + 3], b[offset + 2], b[offset + 1], b[offset + 0]], 0);
        offset += 4;
        return data;
    }

    /// <summary>
    /// Get the divisor to convert price values
    /// </summary>
    private static decimal GetDivisor(uint InstrumentToken)
    {
        uint segment = InstrumentToken & 0xff;
        return segment switch
        {
            // CDS
            3 => 10000000.0m,
            // BCD
            6 => 10000.0m,
            _ => 100.0m,
        };
    }

    /// <summary>
    /// Reads an ltp mode tick from raw binary data
    /// </summary>
    private ITick ReadLTP(byte[] b, ref int offset)
    {
        ITick tick = Activator.CreateInstance<T>();
        tick.InstrumentToken = TickerService<T>.ReadInt(b, ref offset);

        decimal divisor = GetDivisor(tick.InstrumentToken);

        tick.Tradable = (tick.InstrumentToken & 0xff) != 9;
        tick.LastPrice = TickerService<T>.ReadInt(b, ref offset) / divisor;
        return tick;
    }

    /// <summary>
    /// Reads a index's quote mode tick from raw binary data
    /// </summary>
    private static ITick ReadIndexQuote(byte[] b, ref int offset)
    {
        ITick tick = Activator.CreateInstance<T>();

        tick.InstrumentToken = TickerService<T>.ReadInt(b, ref offset);

        decimal divisor = GetDivisor(tick.InstrumentToken);

        tick.Tradable = (tick.InstrumentToken & 0xff) != 9;
        tick.LastPrice = TickerService<T>.ReadInt(b, ref offset) / divisor;
        tick.High = TickerService<T>.ReadInt(b, ref offset) / divisor;
        tick.Low = TickerService<T>.ReadInt(b, ref offset) / divisor;
        tick.OpenPrice = TickerService<T>.ReadInt(b, ref offset) / divisor;
        tick.PreviousDayClosePrice = TickerService<T>.ReadInt(b, ref offset) / divisor;
        tick.Change = TickerService<T>.ReadInt(b, ref offset) / divisor;
        return tick;
    }

    private ITick ReadIndexFull(byte[] b, ref int offset)
    {
        ITick tick = Activator.CreateInstance<T>();
        tick.InstrumentToken = TickerService<T>.ReadInt(b, ref offset);

        decimal divisor = GetDivisor(tick.InstrumentToken);

        tick.Tradable = (tick.InstrumentToken & 0xff) != 9;
        tick.LastPrice = TickerService<T>.ReadInt(b, ref offset) / divisor;
        tick.High = TickerService<T>.ReadInt(b, ref offset) / divisor;
        tick.Low = TickerService<T>.ReadInt(b, ref offset) / divisor;
        tick.OpenPrice = TickerService<T>.ReadInt(b, ref offset) / divisor;
        tick.PreviousDayClosePrice = TickerService<T>.ReadInt(b, ref offset) / divisor;
        // ignore this int
        TickerService<T>.ReadInt(b, ref offset);
        // calculate the change based on existing values
        tick.Change = tick.LastPrice - tick.PreviousDayClosePrice;
        uint time = TickerService<T>.ReadInt(b, ref offset);
        //tick.Timestamp = Utils.UnixToDateTime(time);
        return tick;
    }

    ///// <summary>
    ///// Reads a quote mode tick from raw binary data
    ///// </summary>
    //private Tick ReadQuote(byte[] b, ref int offset)
    //{
    //    Tick tick = new Tick();
    //    tick.Mode = Constants.MODE_QUOTE;
    //    tick.InstrumentToken = TickerService<TEntity>.ReadInt(b, ref offset);

    //    decimal divisor = GetDivisor(tick.InstrumentToken);

    //    tick.Tradable = (tick.InstrumentToken & 0xff) != 9;
    //    tick.LastPrice = TickerService<TEntity>.ReadInt(b, ref offset) / divisor;
    //    tick.LastQuantity = TickerService<TEntity>.ReadInt(b, ref offset);
    //    tick.AveragePrice = TickerService<TEntity>.ReadInt(b, ref offset) / divisor;
    //    tick.Volume = TickerService<TEntity>.ReadInt(b, ref offset);
    //    tick.BuyQuantity = TickerService<TEntity>.ReadInt(b, ref offset);
    //    tick.SellQuantity = TickerService<TEntity>.ReadInt(b, ref offset);
    //    tick.Open = TickerService<TEntity>.ReadInt(b, ref offset) / divisor;
    //    tick.High = TickerService<TEntity>.ReadInt(b, ref offset) / divisor;
    //    tick.Low = TickerService<TEntity>.ReadInt(b, ref offset) / divisor;
    //    tick.Close = TickerService<TEntity>.ReadInt(b, ref offset) / divisor;

    //    return tick;
    //}

    ///// <summary>
    ///// Reads a full mode tick from raw binary data
    ///// </summary>
    //private Tick ReadFull(byte[] b, ref int offset)
    //{
    //    Tick tick = new Tick();
    //    tick.Mode = Constants.MODE_FULL;
    //    tick.InstrumentToken = TickerService<TEntity>.ReadInt(b, ref offset);

    //    decimal divisor = GetDivisor(tick.InstrumentToken);

    //    tick.Tradable = (tick.InstrumentToken & 0xff) != 9;
    //    tick.LastPrice = TickerService<TEntity>.ReadInt(b, ref offset) / divisor;
    //    tick.LastQuantity = TickerService<TEntity>.ReadInt(b, ref offset);
    //    tick.AveragePrice = TickerService<TEntity>.ReadInt(b, ref offset) / divisor;
    //    tick.Volume = TickerService<TEntity>.ReadInt(b, ref offset);
    //    tick.BuyQuantity = TickerService<TEntity>.ReadInt(b, ref offset);
    //    tick.SellQuantity = TickerService<TEntity>.ReadInt(b, ref offset);
    //    tick.Open = TickerService<TEntity>.ReadInt(b, ref offset) / divisor;
    //    tick.High = TickerService<TEntity>.ReadInt(b, ref offset) / divisor;
    //    tick.Low = TickerService<TEntity>.ReadInt(b, ref offset) / divisor;
    //    tick.Close = TickerService<TEntity>.ReadInt(b, ref offset) / divisor;

    //    // KiteConnect 3 fields
    //    tick.LastTradeTime = Utils.UnixToDateTime(TickerService<TEntity>.ReadInt(b, ref offset));
    //    tick.OI = TickerService<TEntity>.ReadInt(b, ref offset);
    //    tick.OIDayHigh = TickerService<TEntity>.ReadInt(b, ref offset);
    //    tick.OIDayLow = TickerService<TEntity>.ReadInt(b, ref offset);
    //    tick.Timestamp = Utils.UnixToDateTime(TickerService<TEntity>.ReadInt(b, ref offset));


    //    tick.Bids = new DepthItem[5];
    //    for (int i = 0; i < 5; i++)
    //    {
    //        tick.Bids[i].Quantity = TickerService<TEntity>.ReadInt(b, ref offset);
    //        tick.Bids[i].Price = TickerService<TEntity>.ReadInt(b, ref offset) / divisor;
    //        tick.Bids[i].Orders = TickerService<TEntity>.ReadShort(b, ref offset);
    //        offset += 2;
    //    }

    //    tick.Offers = new DepthItem[5];
    //    for (int i = 0; i < 5; i++)
    //    {
    //        tick.Offers[i].Quantity = TickerService<TEntity>.ReadInt(b, ref offset);
    //        tick.Offers[i].Price = TickerService<TEntity>.ReadInt(b, ref offset) / divisor;
    //        tick.Offers[i].Orders = TickerService<TEntity>.ReadShort(b, ref offset);
    //        offset += 2;
    //    }
    //    return tick;
    //}

    private void _onData(byte[] Data, int Count, string MessageType)
    {
        _timerTick = RetryInterval;
        if (MessageType == "Binary")
        {
            if (Count == 1)
            {
                if (DebugMode) Console.WriteLine(DateTime.Now.ToLocalTime() + " Heartbeat");
            }
            else
            {
                int offset = 0;
                ushort count = TickerService<T>.ReadShort(Data, ref offset); //number of packets
                if (DebugMode) Console.WriteLine("No of packets: " + count);
                if (DebugMode) Console.WriteLine("No of bytes: " + Count);

                for (ushort i = 0; i < count; i++)
                {
                    ushort length = TickerService<T>.ReadShort(Data, ref offset); // length of the packet
                    if (DebugMode) Console.WriteLine("Packet Length " + length);
                    ITick tick = Activator.CreateInstance<T>();
                    if (length == 8) // ltp
                        tick = ReadLTP(Data, ref offset);
                    else if (length == 28) // index quote
                        tick = TickerService<T>.ReadIndexQuote(Data, ref offset);
                    else if (length == 32) // index quote
                        tick = ReadIndexFull(Data, ref offset);
                    //else if (length == 44) // quote
                    //    tick = ReadQuote(Data, ref offset);
                    //else if (length == 184) // full with marketdepth and timestamp
                    //    tick = ReadFull(Data, ref offset);
                    // If the number of bytes got from stream is less that that is required
                    // data is invalid. This will skip that wrong tick
                    if (tick.InstrumentToken != 0 && IsConnected && offset <= Count)
                    {
                        OnTick(tick);
                    }
                }
            }
        }
        else if (MessageType == "Text")
        {
            string message = Encoding.UTF8.GetString(Data.Take(Count).ToArray());
            if (DebugMode) Console.WriteLine("WebSocket Message: " + message);

            Dictionary<string, dynamic>? messageDict = JsonSerializer.Deserialize<Dictionary<string, dynamic>>(message);
            if (messageDict["type"] == "order")
            {
                //OnOrderUpdate?.Invoke(new Order(messageDict["data"]));
            }
            //else if (messageDict["type"] == "error")
            //{
            //    OnError?.Invoke(messageDict["data"]);
            //}
        }
        else if (MessageType == "Close")
        {
            Close();
        }

    }

    private void _onTimerTick(object? sender, System.Timers.ElapsedEventArgs e)
    {
        // For each timer tick count is reduced. If count goes below 0 then reconnection is triggered.
        _timerTick--;
        if (_timerTick < 0)
        {
            _timer.Stop();
            if (ShouldReconnect)
                Reconnect();
        }
        if (DebugMode) Console.WriteLine(_timerTick);
    }

    private void _onConnect()
    {
        // Reset timer and retry counts and resubscribe to tokens.
        _retryCount = 0;
        _timerTick = RetryInterval;
        _timer.Start();
        if (_subscribedTokens.Count > 0)
            ReSubscribe();
        OnConnect?.Invoke();
    }

    /// <summary>
    /// Tells whether ticker is connected to server not.
    /// </summary>
    public bool IsConnected
    {
        get { return _ws.IsConnected(); }
    }

    /// <summary>
    /// Start a WebSocket connection
    /// </summary>
    public void Connect()
    {
        _timerTick = RetryInterval;
        _timer.Start();
        if (!IsConnected)
        {
            _ws.Connect(SocketUrl, Headers);
        }
    }

    /// <summary>
    /// Close a WebSocket connection
    /// </summary>
    public void Close()
    {
        _timer.Stop();
        _ws.Close();
    }

    /// <summary>
    /// Reconnect WebSocket connection in case of failures
    /// </summary>
    private void Reconnect()
    {
        if (IsConnected)
            _ws.Close(true);

        if (_retryCount > MaxRetryCount)
        {
            _ws.Close(true);
            DisableReconnect();
            OnNoReconnect?.Invoke();
        }
        else
        {
            OnReconnect?.Invoke();
            _retryCount += 1;
            _ws.Close(true);
            Connect();
            _timerTick = (int)Math.Min(Math.Pow(2, _retryCount) * RetryInterval, 60);
            if (DebugMode) Console.WriteLine("New interval " + _timerTick);
            _timer.Start();
        }
    }

    /// <summary>
    /// Subscribe to a list of instrument_tokens.
    /// </summary>
    /// <param name="Tokens">List of instrument instrument_tokens to subscribe</param>
    public void Subscribe(UInt32[] Tokens)
    {
        if (Tokens.Length == 0) return;

        string msg = $"{{  \"t\": \"c\",  \"uid\": \"FT034330\",  \"actid\": \"FT034330\",  \"susertoken\": \"{AccessToken}\",  \"source\": \"WEB\"}}";
        //string msg = "{\"a\":\"subscribe\",\"v\":[" + String.Join(",", Tokens) + "]}";
        if (DebugMode) Console.WriteLine(msg.Length);

        if (IsConnected)
            _ws.Send(msg);

        foreach (uint token in Tokens)
            if (!_subscribedTokens.ContainsKey(token))
                _subscribedTokens.Add(token, "quote");
    }

    /// <summary>
    /// Unsubscribe the given list of instrument_tokens.
    /// </summary>
    /// <param name="Tokens">List of instrument instrument_tokens to unsubscribe</param>
    public void UnSubscribe(UInt32[] Tokens)
    {
        if (Tokens.Length == 0) return;

        string msg = "{\"a\":\"unsubscribe\",\"v\":[" + String.Join(",", Tokens) + "]}";
        if (DebugMode) Console.WriteLine(msg);

        if (IsConnected)
            _ws.Send(msg);
        foreach (uint token in Tokens)
            if (_subscribedTokens.ContainsKey(token))
                _subscribedTokens.Remove(token);
    }

    /// <summary>
    /// Set streaming mode for the given list of tokens.
    /// </summary>
    /// <param name="Tokens">List of instrument tokens on which the mode should be applied</param>
    /// <param name="Mode">Mode to set. It can be one of the following: ltp, quote, full.</param>
    public void SetMode(uint[] Tokens, string Mode)
    {
        if (Tokens.Length == 0) return;

        string msg = "{\"a\":\"mode\",\"v\":[\"" + Mode + "\", [" + String.Join(",", Tokens) + "]]}";
        if (DebugMode) Console.WriteLine(msg);

        if (IsConnected)
            _ws.Send(msg);
        foreach (uint token in Tokens)
            if (_subscribedTokens.ContainsKey(token))
                _subscribedTokens[token] = Mode;
    }

    /// <summary>
    /// Resubscribe to all currently subscribed tokens. Used to restore all the subscribed tokens after successful reconnection.
    /// </summary>
    public void ReSubscribe()
    {
        //if (DebugMode) Console.WriteLine("Resubscribing");
        //UInt32[] all_tokens = _subscribedTokens.Keys.ToArray();

        //UInt32[] ltp_tokens = all_tokens.Where(key => _subscribedTokens[key] == "ltp").ToArray();
        //UInt32[] quote_tokens = all_tokens.Where(key => _subscribedTokens[key] == "quote").ToArray();
        //UInt32[] full_tokens = all_tokens.Where(key => _subscribedTokens[key] == "full").ToArray();

        //UnSubscribe(all_tokens);
        //Subscribe(all_tokens);

        //SetMode(ltp_tokens, "ltp");
        //SetMode(quote_tokens, "quote");
        //SetMode(full_tokens, "full");
    }

    /// <summary>
    /// Enable WebSocket autreconnect in case of network failure/disconnection.
    /// </summary>
    /// <param name="Interval">Interval between auto reconnection attemptes. `onReconnect` callback is triggered when reconnection is attempted.</param>
    /// <param name="Retries">Maximum number reconnection attempts. Defaults to 50 attempts. `onNoReconnect` callback is triggered when number of retries exceeds this value.</param>
    public void EnableReconnect(int Interval = 5, int Retries = 50)
    {
        ShouldReconnect = true;
        RetryInterval = Math.Max(Interval, 5);
        MaxRetryCount = Retries;

        _timerTick = RetryInterval;
        if (IsConnected)
            _timer.Start();
    }

    /// <summary>
    /// Disable WebSocket autreconnect.
    /// </summary>
    public void DisableReconnect()
    {
        ShouldReconnect = false;
        if (IsConnected)
            _timer.Stop();
        _timerTick = RetryInterval;
    }
}
