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
    public string groupId;
    public string roomName;
    public bool isQueueTable;
    public bool isManualLaunch;
    //[HideInInspector]
    //DateTime lastTableUpdateTime;


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
        //SetLastTableUpdateTime(_message);
        OnMessage(_message);      
    }

    private void Awake()
    {
        SetLaunchState(!isManualLaunch);     
    }
    void Subscribing(bool _isSubscribe)
    {

        string path;
        if (_isSubscribe)
            path = "/Subscribe";
        else
            path = "/Unsubscribe";

        Uri uri = new Uri("http://" + WebSocketController.HOST + path);
        HTTPRequest request = new HTTPRequest(uri, HTTPMethods.Post, (HTTPRequest originalRequest, HTTPResponse response) =>
        {
            if (response == null || response.StatusCode != 200)
            {
                Debug.LogError("與伺服器端連接失敗");
                return;
            }

            if (_isSubscribe)
            {
                Debug.Log("註冊訊息：" + response.DataAsText);
                //SetLastTableUpdateTime(response.DataAsText);
                OnSubscribeFinished(response.DataAsText);
            }
            else
            {
                Debug.Log("取消註冊訊息：" + response.DataAsText);
            }
        });

        Dictionary<string, object> req = new Dictionary<string, object>
        {
            { "groupId", groupId },
            { "tableId", roomName },
            { "isQueueTable", isQueueTable },
            { "sessionId", webSocketController.sessionId }
        };

        request.RawData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(req));
        request.Send();
    }

    void OnErrorEvent(string _errorMessage)
    {
        Debug.LogError("Error Message : " + _errorMessage);
        OnError(_errorMessage);
    }


    public void SubscribeTable()
    {
        if (string.IsNullOrEmpty(roomName))
        {
            Debug.LogError("房名不得為空，註冊失敗");
            return;
        }

        if (!webSocketController.webSocket.IsOpen)
        {
            Debug.LogError("與Server端無連接，註冊失敗");
            return;
        }

        string key = string.Format("{0}_{1}", groupId, roomName);
        webSocketController.openEvents.Put(key, OnOpen);
        webSocketController.messageReceivedEvents.Put(key, OnMessageEvent);
        webSocketController.closeEvents.Put(key, OnClose);
        webSocketController.errorEvents.Put(key, OnErrorEvent);

        Subscribing(true);
    }

    public void UnsubscribeTable()
    {
        Subscribing(false);
        string key = string.Format("{0}_{1}", groupId, roomName);
        webSocketController.openEvents.Remove(key);
        webSocketController.messageReceivedEvents.Remove(key);
        webSocketController.closeEvents.Remove(key);
        webSocketController.errorEvents.Remove(key);
    }

    public void SetLaunchState(bool _isLaunch)
    {
        SubscribeMonitor monitor = GetComponent<SubscribeMonitor>();
        if (_isLaunch)
        {
            if (monitor == null)
            {
                gameObject.AddComponent<SubscribeMonitor>();
            }
            else
            {
                monitor.enabled = true;
            }
        }
        else
        {
            if (monitor != null)
            {
                Destroy(monitor);
            }
        }
    }   


    //為了將java的unix time轉成DateTime
    //private static readonly DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    //void SetLastTableUpdateTime(string _msg)
    //{
    //    var jobj = JsonConvert.DeserializeObject<JObject>(_msg);
    //    lastTableUpdateTime = epoch.AddMilliseconds((double)jobj.GetValue("lastUpdateTime"));
    //}

    //DateTime GetLastUpdateTime(string _msg)
    //{
    //    return lastTableUpdateTime;
    //}

}
