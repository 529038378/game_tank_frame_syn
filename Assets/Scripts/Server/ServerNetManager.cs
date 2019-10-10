using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
#if !_CLIENT_
public class ServerNetManager : INetManager
{
    INetManagerCallback m_callback = null;
    public ServerNetManager(INetManagerCallback callback)
    {
        m_callback = callback;
        Init();
    }
    NetworkServerSimple m_server = null;
    public void Init()
    {
        m_server = new NetworkServerSimple();
        m_server.Listen(NetworkPredefinedData.port);
        m_server.RegisterHandler(MsgType.Error, OnError);
        m_server.RegisterHandler(MsgType.Connect, OnConnect);
        m_server.RegisterHandler((short) EventPredefined.MsgType.EMT_ENTITY_EVENT, HandleMsg);
        m_server.RegisterHandler((short) EventPredefined.MsgType.EMT_ENTER_GAME, HandleMsg);

    }
    void OnConnect(NetworkMessage msg)
    {
        Debug.Log("Connected!");
    }
    void OnError(NetworkMessage msg)
    {
        Debug.Log(" net event error ");
    }
    public void Leave()
    {
        m_server.DisconnectAllConnections();
    }
    public void Send(int en_id, short msg_type, MessageBase msg)
    {
        if (!m_dic_en_conn.ContainsKey(en_id))
        {
            return;
        }
        m_dic_en_conn[en_id].Send(msg_type, msg);
    }
    public void BroadCast(int en_id, CEntityEvent ev, bool except_self)
    {
        if (null == ev)
        {
            return;
        }
        foreach (var pair in m_dic_en_conn)
        {
            if (except_self && pair.Key == en_id)
            {
                continue;
            }
            ev.IsLocal = en_id == pair.Key;
            pair.Value.Send((short)EventPredefined.MsgType.EMT_ENTITY_EVENT, ev);
        }
    }

    public void BroadCast(int en_id, short msg_type, MessageBase msg, bool except_self = false)
    {
        foreach(var pair in m_dic_en_conn)
        {
            if (except_self && pair.Key == en_id)
            {
                continue;
            }
            pair.Value.Send(msg_type, msg);
        }
    }
    public void HandleMsg(NetworkMessage msg)
    {
        if (null == m_callback)
        {
            return;
        }
        MessageBase mb = new CEvent();
        msg.ReadMessage<MessageBase>(mb);
        int conn_id = m_server.connections.IndexOf(msg.conn);
        m_callback.HandleMsg(mb, conn_id);
        if (!AddConnectToDic(conn_id, msg.conn))
        {
            //短线重连的处理
        }
    }
    Dictionary<int, NetworkConnection> m_dic_en_conn;
    //return : false - 断线重连，true - 原来链接
    public bool AddConnectToDic(int en_id, NetworkConnection conn)
    {
        bool res = true;
        if (!m_dic_en_conn.ContainsKey(en_id))
        {
            m_dic_en_conn.Add(en_id, conn);
        }
        else
        {
            res = m_dic_en_conn[en_id] == conn;
            if (!res)
            {
                m_dic_en_conn[en_id] = conn;
            }
        }
        return res;
    }
}
#endif