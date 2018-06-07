using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Testing : MonoBehaviour{

    public WebSocketController wsc;
    void OnEnable()
    {
        Kfc.Interactive.RoomCreator.Create<SystemMessageControl>(wsc,transform, "SystemMessage", "SYstemToGM00002", false);
    }
}
