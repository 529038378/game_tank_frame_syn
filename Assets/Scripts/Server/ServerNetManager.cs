using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
#if !_CLIENT_
public class ServerNetManager : INetManager
{
    public void Update()
    {
        if (null == m_server)
        {
            return;
        }
        m_server.Update();
    }
    public int GetConnState()
    {
        if(null == m_server)
        {
            return -1;
        }
        return m_server.connections.Count;
    }

    INetManagerCallback m_callback = null;
    public ServerNetManager(INetManagerCallback callback)
    {
        m_callback = callback;
        m_dic_en_conn = new Dictionary<int, NetworkConnection>();
    }
    NetworkServerSimple m_server = null;
    public void Init()
    {
        m_server = new NetworkServerSimple();
        m_server.RegisterHandler(MsgType.Error, OnError);
        m_server.RegisterHandler(MsgType.Connect, OnConnect);
        m_server.RegisterHandler(MsgType.Disconnect, OnDisconnect);
        m_server.RegisterHandler((short) EventPredefined.MsgType.EMT_ENTITY_CREATE, HandleMsg);
        m_server.RegisterHandler((short) EventPredefined.MsgType.EMT_ENTITY_DESTROY, HandleMsg);
        m_server.RegisterHandler((short) EventPredefined.MsgType.EMT_ENTITY_OP, HandleMsg);
        m_server.RegisterHandler((short) EventPredefined.MsgType.EMT_ENTER_GAME, HandleMsg);
        m_server.RegisterHandler((short) EventPredefined.MsgType.EMT_CLIENT_READY, HandleMsg);
        m_server.RegisterHandler((short) EventPredefined
            .MsgType.EMT_QUIT_GAME, OnQuitInGame);
        if (m_server.Listen(NetworkPredefinedData.port))
        {
            Debug.Log("开始监听");
        }

    }
    void OnQuitInGame(NetworkMessage msg)
    {
        Debug.Log("QuitGame");
        int conn_id = m_server.connections.IndexOf(msg.conn);
        m_dic_en_conn.Remove(conn_id);
        if (m_dic_en_conn.Count <= 0)
        {
            Logic.Instance().LeaveGame();
        }
    }

    void OnDisconnect(NetworkMessage msg)
    {
        Debug.Log("Disconnected!");
        int conn_id = m_server.connections.IndexOf(msg.conn);
        m_dic_en_conn.Remove(conn_id);
        if (m_dic_en_conn.Count <= 0)
        {
            Logic.Instance().LeaveGame();
        }
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
        if (!m_dic_en_conn[en_id].Send(msg_type, msg))
        {

        }
        //         NetworkWriter writer = new NetworkWriter();
        //         CEvent ev = msg as CEvent;
        //         writer.StartMessage((short) ev.GetEventType());
        //         ev.Serialize(writer);
        //         writer.FinishMessage();
        //         if ( !m_dic_en_conn[en_id].SendWriter(writer, en_id))
        //         {
        // 
        //         }
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
            if (!pair.Value.Send((short)ev.GetEventType(), ev))
            {
                Debug.Log(" fail to send msg");
            }
//             NetworkWriter writer = new NetworkWriter();
//             writer.StartMessage((short)ev.GetEventType());
//             ev.Serialize(writer);
//             writer.FinishMessage();
//             if (!pair.Value.SendWriter(writer, en_id))
//             {
//                 Debug.Log(" fail to send msg");
//             }
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

    IEvent ParseEvent(NetworkMessage msg)
    {
        msg.reader.SeekZero();
        IEvent ev = null;
        switch(msg.msgType)
        {
            case (short)EventPredefined.MsgType.EMT_ENTER_GAME:
            ev = new CEnterInGameEvent();
            break;
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
            case (short) EventPredefined.MsgType.EMT_CLIENT_READY:
            ev = new CClientReadyEvent();
            break;
        }
        msg.ReadMessage<MessageBase>(ev);
        return ev;
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
        if (!AddConnectToDic(conn_id, msg.conn))
        {
            //短线重连的处理
        }
       IEvent ev = ParseEvent(msg);
        m_callback.HandleEvent(ev, conn_id);
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