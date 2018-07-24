using UnityEngine;

/// <summary>
/// 用來輔助RoomTemplate的class，可有可無
/// </summary>
public class SubscribeMonitor : MonoBehaviour
{
    bool isSubscribed;
    void OnEnable()
    {
        WebSocketController mController = GetComponent<RoomTemplate>().webSocketController;
        if (mController == null)
        {
            enabled = false;
            Debug.LogWarning("尚未指定wsController");
            return;
        }


        if (string.IsNullOrEmpty(GetComponent<RoomTemplate>().roomName) || string.IsNullOrEmpty(GetComponent<RoomTemplate>().groupId))
        {
            Debug.LogError("roomName或groupId其中一項為空");
            Debug.LogError("roomName : " + GetComponent<RoomTemplate>().roomName);
            Debug.LogError("groupId：" + GetComponent<RoomTemplate>().groupId);
            enabled = false;
            return;
        }

        Debug.Log("開啟自動訂閱桌子 : " + GetComponent<RoomTemplate>().roomName);

        mController.openSocketEvent += HandleResubscribe;
        if (mController.webSocket != null && mController.webSocket.IsOpen)
        {
            HandleResubscribe();        
        }
    }

    void HandleResubscribe()
    {
        isSubscribed = true;
        GetComponent<RoomTemplate>().SubscribeTable();
    }

    void OnDisable()
    {
        if (GetComponent<RoomTemplate>().webSocketController == null) return;
        if (!isSubscribed) return;
        Debug.Log("取消訂閱" + GetComponent<RoomTemplate>().roomName);
        GetComponent<RoomTemplate>().UnsubscribeTable();
        GetComponent<RoomTemplate>().webSocketController.openSocketEvent -= HandleResubscribe;
    }

}
