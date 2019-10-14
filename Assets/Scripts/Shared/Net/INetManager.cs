using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public interface INetManager
{
    void Init();
    void Leave();
    void HandleMsg(NetworkMessage msg);
    int GetConnState();
#if !_CLIENT_
    bool AddConnectToDic(int en_id, NetworkConnection conn);
    void BroadCast(short msg_type, MessageBase msg);
    void Send(int en_id, short msg_type, MessageBase msg);
    void BroadCast(int en_id, CEntityEvent ev, bool except_self);
    void Update();
#else
    void Send(short msg_type, MessageBase msg);
#endif
}

public interface INetManagerCallback
{
#if !_CLIENT_
    void HandleEvent(IEvent msg, int conn_id);
#else
    void HandleEvent(IEvent msg);
#endif
}
