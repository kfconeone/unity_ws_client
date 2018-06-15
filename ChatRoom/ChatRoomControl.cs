using BestHTTP;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class ChatRoomControl : RoomTemplate
{

    public GameObject chatroom;
    public InputField inputField;
    public Transform contentParent;
    public Text textPrefab;
    public Button sendMessageButton;
    //public RawImage connectingPic;


    public string nickName;
    double lastUpdateTime;
    List<JObject> currentTable;

    public override void OnSubscribeFinished(string _json)
    {
        JObject res = JsonConvert.DeserializeObject<JObject>(_json);
        if (res.GetValue("result").ToString() != "000")
        {
            Debug.Log("註冊失敗");
            return;
        }

        //GenerateText("系統訊息", "連接聊天室成功", "ffff00ff");
        //connectingPic.gameObject.SetActive(false);

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
                GenerateText(obj.GetValue("name").ToString(), obj.GetValue("content").ToString(), "ffffffff");
                lastUpdateTime = tempDate;
            }
        }

        
    }

    public override void OnMessage(string _message)
    {
        Debug.Log("這裡是聊天室收到訊息事件");
        Debug.Log("Message : " + _message);
        JObject res = JsonConvert.DeserializeObject<JObject>(_message);

        Debug.Log("收到聊天室訊息 : " + _message);
        JObject pushObj = JsonConvert.DeserializeObject<JObject>(res.GetValue("pushObject").ToString());
        GenerateText(pushObj.GetValue("name").ToString(), pushObj.GetValue("content").ToString(), "ffffffff");
        
    }

    public override void OnError(string _errorMessage)
    {
        Debug.Log("這裡是聊天室連接錯誤事件");
        Debug.Log("Error Message : " + _errorMessage);
        //connectingPic.gameObject.SetActive(true);
        //GenerateText("系統提醒", "網路不穩，正在重新連接聊天室...", "ffff00ff");
    }

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
        req.Add("groupId", groupId);
        req.Add("tableId", roomName);
        req.Add("pushObject", tempChat);

        request.RawData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(req));
        request.Send();
    }

    private void OnSendMsgFinish(HTTPRequest originalRequest, HTTPResponse response)
    {
        if (response == null || response.StatusCode != 200)
        {
            Debug.LogError("與伺服器端連接失敗");
            return;
        }


        Debug.Log("聊天室傳送結束訊息：" + response.DataAsText);
        inputField.text = string.Empty;
        JObject res = JsonConvert.DeserializeObject<JObject>(response.DataAsText);
        string result = res.GetValue("result").ToString();
        if (!result.Contains("000"))
        {
            GenerateText("系統提醒", "網路不穩，正在重新連接聊天室...", "ffff00ff");
        }
    }


    void GenerateText(string _nickName, string _content, string _colorCode)
    {
        Text txt = Instantiate(textPrefab, contentParent);
        txt.transform.SetAsLastSibling();
        txt.gameObject.name = txt.transform.GetSiblingIndex().ToString();
        txt.text = string.Format("<color=#{0}>{1}：{2}</color>", _colorCode, _nickName, _content);
        txt.enabled = true;
    }

    public void FlipChatroom()
    {
        chatroom.SetActive(!chatroom.activeSelf);
    }
}
