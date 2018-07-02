using UnityEngine;

[RequireComponent(typeof(RoomTemplate))]
/// <summary>
/// 用來輔助RoomTemplate的class，可有可無
/// </summary>
public class SubscribeMonitor : MonoBehaviour
{
    void OnEnable()
    {
        WebSocketController mController = GetComponent<RoomTemplate>().webSocketController;
        if (mController == null)
        {
            gameObject.SetActive(false);
            Debug.LogWarning("尚未指定wsController");
            return;
        }


        if (string.IsNullOrEmpty(GetComponent<RoomTemplate>().roomName) || string.IsNullOrEmpty(GetComponent<RoomTemplate>().groupId))
        {
            Debug.LogError("roomName或groupId其中一項為空");
            Debug.LogError("roomName : " + GetComponent<RoomTemplate>().roomName);
            Debug.LogError("groupId：" + GetComponent<RoomTemplate>().groupId);
            gameObject.SetActive(false);
            return;
        }

        Debug.Log("開啟自動訂閱桌子 : " + GetComponent<RoomTemplate>().roomName);

        

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
        if (GetComponent<RoomTemplate>().webSocketController == null) return;

        Debug.Log("取消訂閱" + GetComponent<RoomTemplate>().roomName);
        GetComponent<RoomTemplate>().UnsubscribeTable();
        GetComponent<RoomTemplate>().webSocketController.openEvent -= HandleResubscribe;
    }
}
