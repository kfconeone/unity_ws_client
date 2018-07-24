using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WebSocketMonitor : MonoBehaviour {

    public int ConnectInterval = 5;
    void OnEnable()
    {
        Debug.Log("ws監控器註冊重連事件，並且開始連線。");
        GetComponent<WebSocketController>().errorSocketEvent += HandleReconnectError;
        GetComponent<WebSocketController>().closeSocketEvent += HandleReconnectClose;
        GetComponent<WebSocketController>().OpenWebSocket();
    }

    void HandleReconnectError(string _message)
    {
        Debug.LogError("這裡是錯誤重連事件，等待" + ConnectInterval + "秒後重連");
        StartCoroutine(Reconnecting());
    }

    void HandleReconnectClose(int _code, string _message)
    {
        Debug.LogError("這裡是關閉重連事件，等待" + ConnectInterval + "秒後重連");
        //StartCoroutine(Reconnecting());
    }

    IEnumerator Reconnecting()
    {
        yield return new WaitForSeconds(ConnectInterval);
        GetComponent<WebSocketController>().OpenWebSocket();
    }

    void OnDisable()
    {
        Debug.Log("關閉連線");
        GetComponent<WebSocketController>().errorSocketEvent -= HandleReconnectError;
        GetComponent<WebSocketController>().closeSocketEvent -= HandleReconnectClose;
        GetComponent<WebSocketController>().CloseWebSocket();
    }
}
