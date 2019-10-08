using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ServerNetManager : NetManagerInterface
{
    NetworkServerSimple m_server = null;
    public void Init()
    {
        m_server = new NetworkServerSimple();
        m_server.Listen(NetworkPredefinedData.port);
        m_server.RegisterHandler(MsgType.Error, OnError);
        m_server.RegisterHandler(MsgType.Connect, OnConnect);
    }
    void OnConnect(NetworkMessage msg)
    {
        Debug.Log("Connected!");
    }
    void OnError(NetworkMessage msg)
    {

    }
    public void Quit()
    {
        m_server.DisconnectAllConnections();
    }
    public void Send(short msg_type, MessageBase msg)
    {

    }
    public void HandleMsg(MessageBase msg)
    {

    }
}
