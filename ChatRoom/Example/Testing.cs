using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BestHTTP;
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;

public class Testing : MonoBehaviour {

    public InputField ConnectServerInput;
    public InputField CreateTableInput;
    public InputField SubscribeTableInput;
    public InputField ChatInput;
    public InputField NickName;

    //(暫無使用)考慮分流用
    string zone;
    //當前連線的識別Id
    string sessionId;

    //===================Client 需要處理的部分=======================
    /// <summary>
    /// 連到Server
    /// </summary>
    public void Connect()
    {
        //只有四個事件：連接成功、收到訊息、關閉連接、發生錯誤
        GetComponent<WebSocketController>().openEvent += OnOpen;
        GetComponent<WebSocketController>().messageReceivedEvent += OnMessage;
        GetComponent<WebSocketController>().closeEvent += OnClose;
        GetComponent<WebSocketController>().errorEvent += OnError;

        //連接server
        GetComponent<WebSocketController>().OpenWebSocket();
    }


    public void SubscribeTable()
    {
        Uri path = new Uri("http://35.194.157.51:8081/Subscribe");
        HTTPRequest request = new HTTPRequest(path, HTTPMethods.Post, OnCreateFinished);
        Dictionary<string, object> req = new Dictionary<string, object>();
        req.Add("tableId", SubscribeTableInput.text);
        req.Add("sessionId", sessionId);

        request.RawData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(req));
        request.Send();
    }


    private void OnSubscribeFinished(HTTPRequest originalRequest, HTTPResponse response)
    {
        Debug.Log("回傳訊息：" + response.DataAsText);
    }

    private void OnGUI()
    {
        if (GUILayout.Button("zzzzzzzzzzzzzzzzzzzz"))
        {
            GetComponent<WebSocketController>().CloseWebSocket();
        }
    }



    void OnOpen()
    {
        Debug.Log("這裡是連接成功事件");

    }

    void OnMessage(string _message)
    {
        Debug.Log("這裡是收到訊息事件");
        Debug.Log("Message : " + _message);
        JObject res = JsonConvert.DeserializeObject<JObject>(_message);
        string tableId = res.GetValue("tableId").ToString();
        if (tableId == "CONNECT")
        {
            sessionId = res.GetValue("sessionId").ToString();
        }

        if (tableId == "Room001")
        {
            Debug.Log("Room001的事件觸發");
        }
    }

    void OnClose(int _code,string _message)
    {
        Debug.Log("這裡是連接關閉事件");
        Debug.Log("Code : " + _code + "Message : " + _message);
    }

    void OnError(string _errorMessage)
    {
        Debug.Log("這裡是連接錯誤事件");
        Debug.Log("Error Message : " + _errorMessage);
    }




    //==============以下是聊天室實作=============
    
    public void CreateTable()
    {
        Uri path = new Uri("http://35.194.157.51:8081/Create");
        HTTPRequest request = new HTTPRequest(path, HTTPMethods.Post, OnCreateFinished);
        Dictionary<string, object> req = new Dictionary<string, object>();
        req.Add("tableId", CreateTableInput.text);
        req.Add("detail", new Dictionary<string,object>());

        request.RawData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(req));
        request.Send();

    }

    private void OnCreateFinished(HTTPRequest originalRequest, HTTPResponse response)
    {
        Debug.Log("回傳訊息：" + response.DataAsText);
    }


    public void Chating()
    {
        Uri path = new Uri("http://35.194.157.51:8081/Push");
        HTTPRequest request = new HTTPRequest(path, HTTPMethods.Post, OnUpdateFinished);
        Dictionary<string, object> tempChat = new Dictionary<string, object>();
        tempChat.Add("name",NickName.text);
        tempChat.Add("content", ChatInput.text);
        tempChat.Add("date", DateTime.UtcNow.AddHours(8).Ticks);

        Dictionary<string, object> req = new Dictionary<string, object>();
        req.Add("tableId", SubscribeTableInput.text);
        req.Add("pushObject", tempChat);

        request.RawData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(req));
        request.Send();

    }

    private void OnUpdateFinished(HTTPRequest originalRequest, HTTPResponse response)
    {
        Debug.Log("回傳訊息：" + response.DataAsText);
    }

}
