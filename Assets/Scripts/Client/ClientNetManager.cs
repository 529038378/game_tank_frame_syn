using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
#if _CLIENT_
public class ClientNetManager : INetManager
{
    INetManagerCallback m_callback = null;
    public ClientNetManager(INetManagerCallback callback)
    {
        m_callback = callback;
        Init();
    }

    NetworkClient m_client;
    //继承的接口
    public void Init()
    {
        m_client = new NetworkClient();
        m_client.RegisterHandler(MsgType.Connect, OnConnect);
        m_client.RegisterHandler(MsgType.Error, OnError);
        m_client.RegisterHandler((short)EventPredefined.MsgType.EMT_ENTITY_CHANGE_SCENE, HandleMsg);
        m_client.RegisterHandler((short)EventPredefined.MsgType.EMT_ENTITY_EVENT, HandleMsg);
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
    }
    public void HandleMsg(NetworkMessage msg)
    {
        if (null == m_callback)
        {
            return;
        }
        MessageBase mb = new CEvent();
        msg.ReadMessage<MessageBase>(mb);
        m_callback.HandleMsg(mb);
    }

}
#endif
