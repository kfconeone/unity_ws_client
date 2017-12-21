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

public class ChatRoomControl : MonoBehaviour {

    public InputField inputField;
    public Transform contentParent;
    public Text textPrefab;
    public Button sendMessageButton;
    public WebSocketController webSocketController;
    
    const string CHAT_ROOM = "LobbyChatRoom";
    string nickName;
    

    WebSocket webSocket;
    string sessionId;
    List<JObject> currentTable;
    double lastUpdateTime;



    private void OnEnable()
    {
        //CreateTable();
        webSocket = OpenConnection("Anonymous");
    }

    private void OnDisable()
    {
        CloseConnection();
    }

    private void OnDestroy()
    {
        CloseConnection();
    }

    //================Step 1 : 建立與server端溝通的函式==============
    //當連線成功後，Server端只會有四種訊息：
    //連接成功、關閉連接、收到訊息、發生錯誤
    //OnOpen()、OnClose()、OnMessage()、OnError()
    //所以 Step 2 要將這四個函式註冊到WebSocketController的四個對應的event
    //===============================================================
    void OnOpen()
    {
        Debug.Log("這裡是聊天室WebSocket連接成功事件");
    }

    void OnClose(int _code, string _message)
    {
        Debug.Log("這裡是聊天室連接關閉事件");
        Debug.Log("Code : " + _code + "Message : " + _message);
    }

    void OnMessage(string _message)
    {
        Debug.Log("這裡是聊天室收到訊息事件");
        Debug.Log("Message : " + _message);
        JObject res = JsonConvert.DeserializeObject<JObject>(_message);
        string tableId = res.GetValue("tableId").ToString();
        if (tableId == "CONNECT")
        {
            sessionId = res.GetValue("sessionId").ToString();
            //GenerateText("系統訊息","連接聊天室成功", "ffff00ff");
            SubscribeTable();
        }

        if (tableId == CHAT_ROOM)
        {
            Debug.Log("收到聊天室訊息 : " + _message);
            JObject pushObj = JsonConvert.DeserializeObject<JObject>(res.GetValue("pushObject").ToString());
            GenerateText(pushObj.GetValue("name").ToString(), pushObj.GetValue("content").ToString(),"ffffffff");
        }
    }

    void OnError(string _errorMessage)
    {
        Debug.Log("這裡是聊天室連接錯誤事件");
        Debug.Log("Error Message : " + _errorMessage);
        GenerateText("系統提醒","網路不穩，正在重新連接聊天室...", "ffff00ff");
        StartCoroutine(Reconnect());
        
    }


    IEnumerator Reconnect()
    {
        CloseConnection();
        yield return new WaitForSeconds(5);
        OpenConnection(nickName);
    }


    //================Step 2 : 開啟與關閉ws連線================
    //連線成功訊息跟所有其他訊息一樣是透過OnMessage()來進行傳送
    //=========================================================
    WebSocket OpenConnection(string _nickName)
    {
        nickName = _nickName;
        webSocketController.openEvent += OnOpen;
        webSocketController.messageReceivedEvent += OnMessage;
        webSocketController.closeEvent += OnClose;
        webSocketController.errorEvent += OnError;

        return webSocketController.OpenWebSocket();
    }

    void CloseConnection()
    {
        webSocketController.openEvent -= OnOpen;
        webSocketController.messageReceivedEvent -= OnMessage;
        webSocketController.closeEvent -= OnClose;
        webSocketController.errorEvent -= OnError;
    }

    //================Step 3 : 訂閱桌子================
    //當連線成功後，就要註冊大廳聊天室的session
    //訂閱完畢會獲得當前的桌狀態
    //=================================================
    public void SubscribeTable()
    {
        Uri path = new Uri("http://" + WebSocketController.HOST + "/Subscribe");
        HTTPRequest request = new HTTPRequest(path, HTTPMethods.Post, OnSubscribeFinished);
        Dictionary<string, object> req = new Dictionary<string, object>();
        req.Add("tableId", CHAT_ROOM);
        req.Add("sessionId", sessionId);

        request.RawData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(req));
        request.Send();
    }

    private void OnSubscribeFinished(HTTPRequest originalRequest, HTTPResponse response)
    {
        Debug.Log("聊天室註冊訊息：" + response.DataAsText);
        GenerateText("系統訊息", "連接聊天室成功", "ffff00ff");
        GetTableDetail(response.DataAsText);
    }

    void GetTableDetail(string _json)
    {
        JObject res = JsonConvert.DeserializeObject<JObject>(_json);
        if (res.GetValue("detail").HasValues)
        {
            currentTable = JsonConvert.DeserializeObject<List<JObject>>(res.GetValue("detail").ToString());
        }
        else
        {
            currentTable = new List<JObject>();
            return;
        }

        foreach (JObject obj in currentTable)
        {
            double tempDate = (double)obj.GetValue("date");
            if (tempDate > lastUpdateTime)
            {
                GenerateText(obj.GetValue("name").ToString(), obj.GetValue("content").ToString(),"ffffffff");
                lastUpdateTime = tempDate;
            }
        }
    }

    

    //================Step 4 : 發送訊息與接收訊息================
    //發送訊息也是使用REST的呼叫，但是接受是從OnMessage()
    //所以這裡是負責解析回傳的內容並且顯示在UI上
    //===========================================================
    public void SendMsg()
    {
        Uri path = new Uri("http://" + WebSocketController.HOST + "/Push");
        lastUpdateTime = DateTime.UtcNow.AddHours(8).Ticks;
        HTTPRequest request = new HTTPRequest(path, HTTPMethods.Post, OnSendMsgFinish);
        Dictionary<string, object> tempChat = new Dictionary<string, object>();
        tempChat.Add("name", nickName);
        tempChat.Add("content", inputField.text);
        tempChat.Add("date", lastUpdateTime);

        Dictionary<string, object> req = new Dictionary<string, object>();
        req.Add("tableId", CHAT_ROOM);
        req.Add("pushObject", tempChat);

        request.RawData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(req));
        request.Send();
    }

    private void OnSendMsgFinish(HTTPRequest originalRequest, HTTPResponse response)
    {
        Debug.Log("聊天室傳送結束訊息：" + response.DataAsText);
    }


    void GenerateText(string _nickName,string _content,string _colorCode)
    {
        Text txt = Instantiate(textPrefab, contentParent);
        txt.transform.SetAsLastSibling();
        txt.gameObject.name = txt.transform.GetSiblingIndex().ToString();
        txt.text = string.Format("<color=#{0}>{1}：{2}</color>", _colorCode, _nickName, _content);
        txt.enabled = true;
    }

    public void SetNickName()
    {
        nickName = inputField.text;
    }

}
