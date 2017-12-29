using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChatRoomList : RoomTemplate
{
    public override void OnError(string _message)
    {
        Debug.LogError(_message);
    }

    public override void OnMessage(string _message)
    {
        Debug.LogError(_message);
    }

    public override void OnSubscribeFinished(string _message)
    {
        Debug.LogError(_message);
    }
}
