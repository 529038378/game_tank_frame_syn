using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public interface INetManager
{
    void Init();
    void Leave();
    void HandleMsg(NetworkMessage msg);
#if !_CLIENT_
    bool AddConnectToDic(int en_id, NetworkConnection conn);
    void BroadCast(int en_id, short msg_type, MessageBase msg , bool except_self);
    void Send(int en_id, short msg_type, MessageBase msg);
    void BroadCast(int en_id, CEntityEvent ev, bool except_self);
#else
    void Send(short msg_type, MessageBase msg);
#endif
}

public interface INetManagerCallback
{
#if !_CLIENT_
    void HandleMsg(MessageBase msg, int conn_id);
#else
    void HandleMsg(MessageBase msg);
#endif
}
