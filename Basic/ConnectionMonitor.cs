using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectionMonitor : MonoBehaviour {

	void OnEnable () {
        Debug.Log("開啟連線");
        GetComponent<RoomTemplate>().errorEvent += HandleReconnection;
        GetComponent<RoomTemplate>().OpenConnection();
	}

    void HandleReconnection()
    {
        Debug.LogError("這裡是重連事件，等待五秒後重連");
        StartCoroutine(Reconnect());
    }

    IEnumerator Reconnect()
    {
        GetComponent<RoomTemplate>().CloseConnection();
        yield return new WaitForSeconds(5);
        GetComponent<RoomTemplate>().OpenConnection();
    }

    void OnDisable () {
        Debug.Log("關閉連線");
        GetComponent<RoomTemplate>().errorEvent -= HandleReconnection;
        GetComponent<RoomTemplate>().CloseConnection();
    }
}
