using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class CSceneMng : ISceneMng, INetManagerCallback
{
    bool m_is_scene;
    public override bool InScene
    {
        get
        {
            return m_is_scene;
        }
    }
#if _CLIENT_
    public void HandleMsg(MessageBase msg)
    {
        IEvent ev = msg as IEvent;
        HandleEvent(ev);
    }
#else
    public void HandleMsg(MessageBase msg, int conn_id)
    {
        IEvent ev = msg as IEvent;
        HandleEvent(ev, conn_id);
    }
#endif
    //int - en_id, IEntity - concreta_en
    Dictionary<int, IEntity> m_dic_ens;
    //int - en_id, IEntity - recycle_en
    Dictionary<int, IEntity> m_recyle_ens;


    public override void Enter()
    {
        m_is_scene = true;
        m_dic_ens = new Dictionary<int, IEntity>();
        m_recyle_ens = new Dictionary<int, IEntity>();
    }

    public override void Leave()
    {
        m_is_scene = false;
        m_dic_ens.Clear();
        m_recyle_ens.Clear();
    }

    //创建实体
    void CreateEn(IEvent ev)
    {
        CCreateEvent cce = ev as CCreateEvent;
        if (null == cce)
        {
            return;
        }
        IEntity en = null;
        switch(cce.EnType)
        {
            case EntityPredefined.EntityType.EET_TANK:
          en = new TankEntity(cce.EnId);  
            break;
        }
        m_dic_ens.Add(cce.EnId, en);
#if _CLIENT_
        if (cce.IsLocal)
        {
            Logic.Instance().SetOpEn(en);
        }
#endif
    }

    //实体操作
    void EnOp(IEvent ev)
    {
        COpEvent coe = ev as COpEvent;
        if (null == coe)
        {
            return;
        }

        if (!m_dic_ens.ContainsKey(coe.EnId))
        {
            return;
        }

        IEntity en = m_dic_ens[coe.EnId];
        en.Op(coe.OpType);
    }

    //摧毁实体
    void DestoryOp(IEvent ev)
    {
        CDestoryEvent cde = ev as CDestoryEvent;
        if (null == cde)
        {
            return;
        }
        if(!m_dic_ens.ContainsKey(cde.EnId))
        {
            return;
        }
        IEntity en = m_dic_ens[cde.EnId];
        m_dic_ens.Remove(cde.EnId);
        en.Destroy();
    }

    //key : frame_index, value : CRecordEventS
    Dictionary<int, CEntityEvent> m_record_evs;

#if !_CIENT_
    void RecordEv(IEvent ev)
    {
        CEntityEvent re = ev as CEntityEvent;
        if (null == re)
        {
            return;
        }
        m_record_evs.Add(re.FrameIndex, re);

    }
#endif
    void HandleEntityEvent(IEvent ev)
    {
        CEntityEvent cre = ev as CEntityEvent;
        if (null == cre)
        {
            return;
        }
        switch (cre.GetEntityEventType())
        {
            case EventPredefined.EntityEventType.ET_CREATE:
            CreateEn(ev);
            break;
            case EventPredefined.EntityEventType.ET_OP:
            EnOp(ev);
            break;
            case EventPredefined.EntityEventType.ET_DESTROY:
            DestoryOp(ev);
            break;
            default:
            Debug.Assert(false);
            break;
        }
#if !_CLIENT_
        //服务器端要将这个世界记录下来，用于短线重连等相关表现
        RecordEv(ev);
#endif
    }
#if !_CLIENT_
    //给各客户端派发实体创建消息
    void CreatePlayerObjForAllClient()
    {
        INetManager net_mng = Logic.Instance().GetNetMng();
        if (null == net_mng)
        {
            return;
        }
        int index = 0;
        foreach (var conn_id in m_list_player_conn_index)
        {
            CEntityEvent ev = new CCreateEvent(0, conn_id, EntityPredefined.EntityType.EET_TANK, EntityPredefined.EntityCampType.ECT_PLAYER, index);
            net_mng.BroadCast(conn_id, ev, false);
            index++;
        }
    }
    
    //服务器切场景
    void ServerEnterInGame()
    {
        Logic.Instance().EnterInGame();
        //创建实体
        int index = 0;
        foreach (var en_id in m_list_player_conn_index)
        {
            IEvent ev = new CCreateEvent(0, en_id, EntityPredefined.EntityType.EET_TANK, EntityPredefined.EntityCampType.ECT_PLAYER, index);
            CreateEn(ev);
            index++;
        }
       
    }

    //让想入场的玩家先进入局内
    void NotifyToChangeScene(int en_id)
    {
        INetManager net_mng = Logic.Instance().GetNetMng();
        if (null == net_mng)
        {
            return;
        }
        net_mng.Send(en_id, (short)EventPredefined.MsgType.EMT_ENTITY_CHANGE_SCENE, new CEvent());
    }

    //等待玩家入场
    List<int> m_list_player_conn_index;
    void HandleEnterInGame(IEvent ev, int en_id)
    {
        m_list_player_conn_index.Add(en_id);
        
        if(EventPredefined.max_player == m_list_player_conn_index.Count)
        {
            CreatePlayerObjForAllClient();
            ServerEnterInGame();
        }
    }
#endif

#if _CLIENT_
    //处理网络事件
    void ChangeToInGameScene()
    {
        Logic.Instance().EnterInGame();
    }
    public override void HandleEvent(IEvent ev)
    {
        if (null == ev)
        {
            return;
        }
        switch (ev.GetEventType())
        {
            case EventPredefined.MsgType.EMT_ENTITY_EVENT:
            HandleEntityEvent(ev);
            break;
            case EventPredefined.MsgType.EMT_ENTITY_CHANGE_SCENE:
            ChangeToInGameScene();
            break;
            default:
            Debug.Log("wrong event type");
            Debug.Assert(false);
            break;
        }
    }
#else
    public override void HandleEvent(IEvent ev, int en_id)
    {
        if(null == ev)
        {
            return;
        }
        switch (ev.GetEventType())
        {
            case EventPredefined.MsgType.EMT_ENTITY_EVENT:
            HandleEntityEvent(ev);
            return;
            case EventPredefined.MsgType.EMT_ENTER_GAME:
            HandleEnterInGame(ev, en_id);
            break;
            default:
            Debug.Log("wrong event type");
            Debug.Assert(false);
            break;
        }
    }
#endif

    public override void Update()
    {
        foreach (var pair in m_dic_ens)
        {
            IEntity en = pair.Value as IEntity;
            if (null == en)
            {
                continue;
            }
            en.Update();
        }

        foreach(var pair in m_recyle_ens)
        {
            IEntity en = pair.Value as IEntity;
            if(null == en)
            {
                continue;
            }
            en.Destroy();
        }
        m_recyle_ens.Clear();
    }
}
