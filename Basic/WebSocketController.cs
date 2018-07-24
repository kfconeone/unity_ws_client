using System.Linq;
using System.Collections;
using System.Collections.Generic;
using BestHTTP;
using BestHTTP.WebSocket;
using UnityEngine;
using UnityEngine.Events;
using System;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

public class WebSocketController : MonoBehaviour {



    /// <summary>
    /// Saved WebSocket instance
    /// </summary>
    [HideInInspector]
    public WebSocket webSocket;
    
    //剩下這三個事件專門給斷線重連專用
    public event Action openSocketEvent
    {
        add
        {
            if (mOpenSocketEvent == null || !mOpenSocketEvent.GetInvocationList().Contains(value))
            {
                mOpenSocketEvent += value;
            }
        }
        remove
        {
            mOpenSocketEvent -= value;
        }
    }
    event Action mOpenSocketEvent;
    //public event Action<string,string> messageReceivedEvent;
    public event Action<int,string> closeSocketEvent;
    public event Action<string> errorSocketEvent;

    //以下是給使用者量產使用的事件，原本是用上方的事件處理，嘗試改為Dictionary的方式，key為"[groupId]_[tableId]"
    public Dictionary<string, Action> openEvents
    {
        get
        {
            if (mOpenEvents == null)
            {
                mOpenEvents = new Dictionary<string, Action>();
            }
            return mOpenEvents;
        }
    }
    public Dictionary<string, Action<string>> messageReceivedEvents
    {
        get
        {
            if (mMessageReceivedEvents == null)
            {
                mMessageReceivedEvents = new Dictionary<string, Action<string>>();
            }
            return mMessageReceivedEvents;
        }
    }
    public Dictionary<string, Action<int, string>> closeEvents
    {
        get
        {
            if (mCloseEvents == null)
            {
                mCloseEvents = new Dictionary<string, Action<int, string>>();
            }

            return mCloseEvents;
        }
    }

    public Dictionary<string, Action<string>> errorEvents
    {
        get
        {
            if (mErrorEvents == null)
            {
                mErrorEvents = new Dictionary<string, Action<string>>();
            }
            return mErrorEvents;
        }
    }

    Dictionary<string, Action> mOpenEvents;
    Dictionary<string, Action<string>> mMessageReceivedEvents;
    Dictionary<string, Action<int, string>> mCloseEvents;
    Dictionary<string, Action<string>> mErrorEvents;

    [HideInInspector]
    public string sessionId;

#if __LOCAL_HOST__
    public const string HOST = "localhost:8081";
#else
    public const string HOST = "entrance10.mobiusdice.com.tw:700";
#endif
    //ws://localhost:8081/interactive
    // Use this for initialization
    public WebSocket OpenWebSocket() {
        // Create the WebSocket instance
        if (webSocket != null) return webSocket;

        webSocket = new WebSocket(new Uri("ws://" + HOST + "/interactive"));

        webSocket.StartPingThread = true;
#if !BESTHTTP_DISABLE_PROXY && !UNITY_WEBGL
        if (HTTPManager.Proxy != null)
            webSocket.InternalRequest.Proxy = new HTTPProxy(HTTPManager.Proxy.Address, HTTPManager.Proxy.Credentials, false);
#endif

        // Subscribe to the WS events
        webSocket.OnOpen += OnOpen;
        webSocket.OnMessage += OnMessageReceived;
        webSocket.OnClosed += OnClosed;
        webSocket.OnError += OnError;

        Debug.Log("開始連接伺服器：" + "ws://" + HOST + "/interactive");
        // Start connecting to the server


        webSocket.Open();

        return webSocket;
    }

    public void CloseWebSocket()
    {
        if (webSocket != null)
        {
            webSocket.Close();    
        }
        //openEvent = null;
        //messageReceivedEvent = null;
        //closeEvent = null;
        //errorEvent = null;
        if(mOpenEvents != null) mOpenEvents.Clear();
        if(mMessageReceivedEvents != null) mMessageReceivedEvents.Clear();
        if(mCloseEvents != null) mCloseEvents.Clear();
        if (mErrorEvents != null) mErrorEvents.Clear();

        webSocket = null;
    }

    void OnDestroy()
    {
        CloseWebSocket();
    }

#region WebSocket Event Handlers

    /// <summary>
    /// Called when the web socket is open, and we are ready to send and receive data
    /// </summary>
    void OnOpen(WebSocket ws)
    {
        Debug.Log(string.Format("連接成功\n"));
        //if(openEvent != null)   openEvent.Invoke();
    }

    /// <summary>
    /// Called when we received a text message from the server
    /// </summary>
    void OnMessageReceived(WebSocket ws, string message)
    {
        JObject res = JsonConvert.DeserializeObject<JObject>(message);

        string messageType = res.GetValue("messageType").ToString();
        if (messageType.Equals("CONNECTED"))   //先檢查此次訊息的類型是否為連接事件
        {
            sessionId = res.GetValue("sessionId").ToString();
            Debug.Log("當前連線的sessionId : " + sessionId);

            //給完sessionId才是"名義上"被認可的連接成功，因此openEvent在此執行而不是OnOpen
            if (mOpenSocketEvent != null) mOpenSocketEvent.Invoke();

            return;
        }


        string groupId = res.GetValue("groupId").ToString();
        string tableId = res.GetValue("tableId").ToString();
        string key = string.Format("{0}_{1}",groupId,tableId);
        if (messageReceivedEvents.ContainsKey(key)) messageReceivedEvents[key](message);
    }

    /// <summary>
    /// Called when the web socket closed
    /// </summary>
    void OnClosed(WebSocket ws, UInt16 code, string message)
    {
        foreach (Action<int, string> closeEvent in closeEvents.Values)
        {
            if (closeEvent != null) closeEvent.Invoke(code, message);
        }

        webSocket = null;
    }

    /// <summary>
    /// Called when an error occured on client side
    /// </summary>
    void OnError(WebSocket ws, Exception ex)
    {
        string errorMsg = string.Empty;
#if !UNITY_WEBGL || UNITY_EDITOR
        if (ws.InternalRequest.Response != null)
            errorMsg = string.Format("Status Code: {0} Message: {1}", ws.InternalRequest.Response.StatusCode, ws.InternalRequest.Response.Message);
#endif

        Debug.Log(string.Format("連線發生錯誤: {0}\n", (ex != null ? ex.Message : "Unknown Error " + errorMsg)));
        if (ex != null)
        {
            //這是桌資料的錯誤事件處理
            foreach (Action<string> err in errorEvents.Values)
            {
                if (err != null) err.Invoke(ex.Message);
            }
        }
        //這是斷線重連的錯誤處理
        if (errorSocketEvent != null) errorSocketEvent((ex != null ? ex.Message : "Unknown Error " + errorMsg));
        webSocket = null;
    }

#endregion
}

