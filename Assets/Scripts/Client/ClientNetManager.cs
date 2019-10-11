using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;
#if _CLIENT_
public class ClientNetManager : INetManager
{
    public int GetConnState()
    {
        if(null == m_client)
        {
            return -1;
        }
        return m_client.isConnected ? 1 : 0;
    }

    INetManagerCallback m_callback = null;
    public ClientNetManager(INetManagerCallback callback)
    {
        m_callback = callback;
    }

    NetworkClient m_client;
    //继承的接口
    public void Init()
    {
        m_client = new NetworkClient();
        m_client.RegisterHandler(MsgType.Connect, OnConnect);
        m_client.RegisterHandler(MsgType.Error, OnError);
        m_client.RegisterHandler((short)EventPredefined.MsgType.EMT_ENTITY_CHANGE_SCENE, HandleMsg);
        m_client.RegisterHandler((short)EventPredefined.MsgType.EMT_ENTITY_CREATE, HandleMsg);
        m_client.RegisterHandler((short) EventPredefined.MsgType.EMT_ENTITY_DESTROY
, HandleMsg);
        m_client.RegisterHandler((short) EventPredefined.MsgType.EMT_ENTITY_OP, HandleMsg);
        m_client.Connect(NetworkPredefinedData.server_ip, NetworkPredefinedData.port);
    }

    void OnError(NetworkMessage msg)
    {
        Debug.Log(" net event error ");
    }
    void OnConnect(NetworkMessage msg)
    {
        Debug.Log(" Connected! ");
    }
    public void Leave()
    {
        m_client.Disconnect();
    }
    public void Send(short msg_type, MessageBase msg)
    {
        m_client.Send(msg_type, msg);
        return;
        CEvent ev = msg as CEvent;
        NetworkWriter writer = new NetworkWriter();
        writer.StartMessage((short)ev.GetEventType());
        ev.Serialize(writer);
        writer.FinishMessage();
        m_client.SendWriter(writer, 0);
    }

    IEvent ParseEvent(NetworkMessage msg)
    {
        IEvent ev = null;
        switch (msg.msgType)
        {
            case (short) EventPredefined.MsgType.EMT_ENTITY_CHANGE_SCENE:
            ev = new CEnterChangeSceneEvent();
            break;
            case (short) EventPredefined.MsgType.EMT_ENTITY_CREATE:
            ev = new CCreateEvent();
            break;
            case (short) EventPredefined.MsgType.EMT_ENTITY_DESTROY:
            ev = new CDestoryEvent();
            break;
            case (short) EventPredefined.MsgType.EMT_ENTITY_OP:
            ev = new COpEvent();
            break;
        }
        //ev.Deserialize(msg.reader);
        msg.ReadMessage<MessageBase>(ev);
        return ev;
    }

    public void HandleMsg(NetworkMessage msg)
    {
        if (null == m_callback)
        {
            return;
        }
        IEvent ev = ParseEvent(msg);
        m_callback.HandleEvent(ev);
    }
}
#endif
