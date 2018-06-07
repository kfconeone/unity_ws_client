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
        if (string.IsNullOrEmpty(GetComponent<RoomTemplate>().roomName) || string.IsNullOrEmpty(GetComponent<RoomTemplate>().groupId))
        {
            Debug.LogError("roomName或groupId其中一項為空");
            Debug.LogError("roomName : " + GetComponent<RoomTemplate>().roomName);
            Debug.LogError("groupId：" + GetComponent<RoomTemplate>().groupId);
            gameObject.SetActive(false);
            return;
        }

        Debug.Log("開啟自動訂閱桌子 : " + GetComponent<RoomTemplate>().roomName);
        WebSocketController mController = GetComponent<RoomTemplate>().webSocketController;

        mController.openEvent += HandleResubscribe;
        if (mController.webSocket != null && mController.webSocket.IsOpen)
        {
            GetComponent<RoomTemplate>().SubscribeTable();
        }
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
