using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WebSocketLiveKeeper : MonoBehaviour
{

    WebSocketController wsController;
    int liveCount;
    private void OnEnable()
    {
        wsController = GetComponent<WebSocketController>();
        liveCount = 0;
        InvokeRepeating("Counting", 0.1f, 7f);
    }

    private void OnDisable()
    {
        liveCount = 0;
    }

    void Counting()
    {
        liveCount+= 7;
        if (liveCount == 21)
        {
            liveCount = 0;
            if (wsController.webSocket.IsOpen)
            {
                wsController.webSocket.Send("keepalive");
            }
        }
    }

    public void StillAlive()
    {
        liveCount = 0;
    }
}
