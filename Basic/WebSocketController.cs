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

    public event Action openEvent;
    public event Action<string,bool> messageReceivedEvent;
    public event Action<int,string> closeEvent;
    public event Action<string> errorEvent;

    [HideInInspector]
    public string sessionId;
    public const string HOST = "35.189.187.45:8081";
    //public const string HOST = "localhost:8081";
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
        openEvent = null;
        messageReceivedEvent = null;
        closeEvent = null;
        errorEvent = null;

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
        string tableId = res.GetValue("tableId").ToString();
        if (tableId == "CONNECT")
        {
            sessionId = res.GetValue("sessionId").ToString();
            Debug.Log("當前連線的sessionId : " + sessionId);
            if (openEvent != null) openEvent.Invoke();
            return;
        }
        if (messageReceivedEvent != null) messageReceivedEvent.Invoke(message, (tableId == "CONNECT"));
    }

    /// <summary>
    /// Called when the web socket closed
    /// </summary>
    void OnClosed(WebSocket ws, UInt16 code, string message)
    {
        if (closeEvent != null) closeEvent.Invoke(code, message);
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
        if (errorEvent != null) errorEvent.Invoke(ex.Message);
        webSocket = null;
    }

    #endregion
}

