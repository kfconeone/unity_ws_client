using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using System;
using BestHTTP;
using BestHTTP.WebSocket;
using System.Text;
using Newtonsoft.Json.Linq;

public abstract class RoomTemplate : MonoBehaviour {

    public WebSocketController webSocketController;
    public string roomName;

    WebSocket webSocket;
    string sessionId;

    public abstract void OnMessage(string _message);
    public abstract void OnError(string _message);
    public abstract void OnSubscribeFinished(string _message);
    public virtual void OnClose(int _code,string _message)
    {
        Debug.LogError(string.Format("OnClose - code : {0} message : {1}",_code,_message));
    }
    public virtual void OnOpen()
    {
        Debug.Log("OnOpen");
    }

    void OnMessageEvent(string _message)
    {
        JObject res = JsonConvert.DeserializeObject<JObject>(_message);
        string tableId = res.GetValue("tableId").ToString();
        if (tableId == "CONNECT")
        {
            sessionId = res.GetValue("sessionId").ToString();
            SubscribeTable(true);
        }
        else
        {
            OnMessage(_message);
        }
    }

    void SubscribeTable(bool _isSubscribe)
    {
        string path;
        if (_isSubscribe)
            path = "/Subscribe";
        else
            path = "/Unsubscribe";

        Uri uri = new Uri("http://" + WebSocketController.HOST + path);
        HTTPRequest request = new HTTPRequest(uri, HTTPMethods.Post, (HTTPRequest originalRequest, HTTPResponse response) =>
        {

            if (_isSubscribe)
            {
                Debug.Log("註冊訊息：" + response.DataAsText);
                OnSubscribeFinished(response.DataAsText);
            }
            else
            {
                Debug.Log("取消註冊訊息：" + response.DataAsText);
            }
        });

        Dictionary<string, object> req = new Dictionary<string, object>();
        req.Add("tableId", roomName);
        req.Add("sessionId", sessionId);

        request.RawData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(req));
        request.Send();
    }

    void OnErrorEvent(string _errorMessage)
    {
        Debug.LogError("這裡是連接錯誤事件，等待五秒後重連");
        Debug.LogError("Error Message : " + _errorMessage);
        OnError(_errorMessage);
        StartCoroutine(Reconnect());
    }

    IEnumerator Reconnect()
    {
        CloseConnection();
        yield return new WaitForSeconds(5);
        OpenConnection();
    }

    public virtual void Push(string _url,object _pushObject,Action<string> _callback)
    {
        Uri uri = new Uri(_url);
        HTTPRequest request = new HTTPRequest(uri, HTTPMethods.Post, (HTTPRequest originalRequest, HTTPResponse response) =>
        {
            _callback(response.DataAsText);
        });
        Dictionary<string, object> req = new Dictionary<string, object>();
        req.Add("tableId", roomName);
        req.Add("pushObject", sessionId);

        request.RawData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(req));
        request.Send();
    }

    protected WebSocket OpenConnection()
    {
        if (string.IsNullOrEmpty(roomName))
        {
            Debug.LogError("房名不得為空，連接失敗");
            return null;
        }

        if (webSocketController == null)
        {
            Debug.LogError("Controller沒有指定，連接失敗");
            return null;
        }

        webSocketController.openEvent += OnOpen;
        webSocketController.messageReceivedEvent += OnMessageEvent;
        webSocketController.closeEvent += OnClose;
        webSocketController.errorEvent += OnErrorEvent;

        webSocket = webSocketController.OpenWebSocket();
        return webSocket;
    }

    protected void CloseConnection()
    {
        if (webSocket != null)
        {
            if (webSocket.IsOpen)
            {
                SubscribeTable(false);
            }
            else
            {
                webSocket = null;
            }
        }

        webSocketController.openEvent -= OnOpen;
        webSocketController.messageReceivedEvent -= OnMessageEvent;
        webSocketController.closeEvent -= OnClose;
        webSocketController.errorEvent -= OnErrorEvent;
    }

}
