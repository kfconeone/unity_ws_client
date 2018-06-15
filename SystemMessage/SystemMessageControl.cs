using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class SystemMessageControl : RoomTemplate
{
    public Dictionary<string, object> systemMessage;

    public override void OnError(string _message)
    {
        Debug.LogError("錯誤訊息：" + _message);
    }

    public override void OnMessage(string _message)
    {
        Debug.LogError("收到訊息：" + _message);

        JObject jsonObj = JsonConvert.DeserializeObject<JObject>(_message);
        systemMessage = jsonObj.GetValue("detail").ToObject<Dictionary<string, object>>();
    }

    public override void OnSubscribeFinished(string _message)
    {
        Debug.LogError("註冊完畢：" + _message);

        JObject jsonObj = JsonConvert.DeserializeObject<JObject>(_message);
        systemMessage = jsonObj.GetValue("detail").ToObject<Dictionary<string, object>>();
        
    }
}
