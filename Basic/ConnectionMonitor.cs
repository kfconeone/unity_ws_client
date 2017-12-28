using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 用來輔助RoomTemplate的class，可有可無
/// </summary>
public class ConnectionMonitor : MonoBehaviour {

    public int reconnectInterval = 5;
	void OnEnable () {
        Debug.Log("開啟連線");
        GetComponent<RoomTemplate>().errorEvent += HandleReconnection;
        GetComponent<RoomTemplate>().OpenConnection();
	}

    void HandleReconnection()
    {
        Debug.LogError("這裡是重連事件，等待" + reconnectInterval + "秒後重連");
        StartCoroutine(Reconnect());
    }

    IEnumerator Reconnect()
    {
        GetComponent<RoomTemplate>().CloseConnection();
        yield return new WaitForSeconds(reconnectInterval);
        GetComponent<RoomTemplate>().OpenConnection();
    }

    void OnDisable () {
        Debug.Log("關閉連線");
        GetComponent<RoomTemplate>().errorEvent -= HandleReconnection;
        GetComponent<RoomTemplate>().CloseConnection();
    }
}
