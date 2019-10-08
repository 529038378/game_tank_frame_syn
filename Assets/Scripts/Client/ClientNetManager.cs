using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ClientNetManager : NetManagerInterface
{
    NetworkClient m_client;
    //继承的接口
    public void Init()
    {
        m_client = new NetworkClient();
        m_client.RegisterHandler(MsgType.Connect, OnConnect);
        m_client.RegisterHandler(MsgType.Error, OnError);
        m_client.Connect(NetworkPredefinedData.server_ip, NetworkPredefinedData.port);
    }
    void OnError(NetworkMessage msg)
    {

    }
    void OnConnect(NetworkMessage msg)
    {
        Debug.Log(" Connected! ");
    }
    public void Quit()
    {
        m_client.Disconnect();
    }
    public void Send(short msg_type, MessageBase msg)
    {

    }
    public void HandleMsg(MessageBase msg)
    {

    }

}
