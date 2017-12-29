using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 用來輔助RoomTemplate的class，可有可無
/// </summary>
public class SubscribeMonitor : MonoBehaviour
{
    void OnEnable()
    {
        Debug.Log("開啟自動訂閱桌子 : " + GetComponent<RoomTemplate>().roomName);
        GetComponent<RoomTemplate>().webSocketController.openEvent += HandleResubscribe;
    }

    void HandleResubscribe()
    {
        GetComponent<RoomTemplate>().SubscribeTable();
    }

    void OnDisable()
    {
        Debug.Log("取消訂閱" + GetComponent<RoomTemplate>().roomName);
        GetComponent<RoomTemplate>().UnsubscribeTable();
        GetComponent<RoomTemplate>().webSocketController.openEvent -= HandleResubscribe;
    }
}
