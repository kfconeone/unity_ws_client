using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class SystemMessageControl : RoomTemplate
{
    public Dictionary<string, object> currentSystemMessage;
    //各項系統訊息事件
    public Dictionary<string, Action> systemMessageEvent
    {
        get
        {
            if (mSstemMessageEvent == null)
            {
                mSstemMessageEvent = new Dictionary<string, Action>();
            }
            return mSstemMessageEvent;
        }
    }
    Dictionary<string, Action> mSstemMessageEvent;


    public override void OnError(string _message)
    {
        Debug.LogError("錯誤訊息：" + _message);
    }

    public override void OnMessage(string _message)
    {
        Debug.LogError("收到訊息：" + _message);
        CheckSystemMessage(_message);
    }

    public override void OnSubscribeFinished(string _message)
    {
        Debug.LogError("註冊完畢：" + _message);
        CheckSystemMessage(_message);
    }

    void CheckSystemMessage(string _message)
    {
        Dictionary<string, object> olsSystemMessage = null;
#if __Debug__
        //PlayerPrefs.DeleteKey("systemMessage");
#endif
        //先取得舊的systemMessage
        bool hasOldSystemMessage = PlayerPrefs.HasKey("systemMessage");
        if (hasOldSystemMessage)
        {
            JObject oldJsonObj = JsonConvert.DeserializeObject<JObject>(PlayerPrefs.GetString("systemMessage"));
            olsSystemMessage = oldJsonObj.GetValue("detail").ToObject<Dictionary<string, object>>();
        }

        //接下來取得新的systemMessage
        PlayerPrefs.SetString("systemMessage", _message);
        JObject jsonObj = JsonConvert.DeserializeObject<JObject>(_message);
        currentSystemMessage = jsonObj.GetValue("detail").ToObject<Dictionary<string, object>>();

        //如果有舊的就先進行比對，如果是新的就無須通知
        if (hasOldSystemMessage)
        {
            foreach (string key in systemMessageEvent.Keys)
            {
                CheckSystemMessageUpdated(key, systemMessageEvent[key], olsSystemMessage);
            }
        }

    }

    void CheckSystemMessageUpdated(string _key,Action _event, Dictionary<string, object> _olsSystemMessage)
    {
        //=====檢查聊天室信號=====
        //先檢查新的系統訊息有沒有
        if (currentSystemMessage.ContainsKey(_key))
        {
            if (!_olsSystemMessage.ContainsKey(_key))
            {
                Debug.Log("即時系統提醒 : " + _key);
                if (_event != null) _event();
            }
            else
            {
                if ((double)currentSystemMessage[_key] > (double)_olsSystemMessage[_key])
                {
                    Debug.Log("即時系統提醒 : " + _key);
                    if (_event != null) _event();
                }
            }
        }
    }
}
