using System;

using UnityEngine;

namespace Kfc.Interactive
{
    public class RoomCreator
    {
        public static void Create<T>(WebSocketController _Connector,Transform _parent,string _groupId,string _tableId,bool _isQueueTable) where T : RoomTemplate
        {
            GameObject roomObject = new GameObject(_groupId + "_" + _tableId);
            roomObject.transform.parent = _parent;
            T smc = roomObject.AddComponent<T>();
            smc.webSocketController = _Connector;
            smc.groupId = _groupId;
            smc.roomName = _tableId;
            smc.isQueueTable = _isQueueTable;

            roomObject.AddComponent<SubscribeMonitor>();
        }
   }
}
