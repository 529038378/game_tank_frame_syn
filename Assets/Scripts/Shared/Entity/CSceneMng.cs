using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class CSceneMng : ISceneMng, INetManagerCallback
{
    public CSceneMng()
    {
#if !_CLIENT_
        m_list_player_conn_index = new List<int>();
#endif
    }
    bool m_is_scene;
    public override bool InScene
    {
        get
        {
            return m_is_scene;
        }
    }
    //int - en_id, IEntity - concreta_en
    Dictionary<int, IEntity> m_dic_ens;
    //int - en_id, IEntity - recycle_en
    Dictionary<int, IEntity> m_recyle_ens;


    public override void Enter()
    {
        m_is_scene = true;
        m_dic_ens = new Dictionary<int, IEntity>();
        m_recyle_ens = new Dictionary<int, IEntity>();
#if _CLIENT_
        Logic.Instance().NotifyClientReady();
#else
        m_ready_player_count = 0;
        m_list_player_conn_index.Clear();
#endif
    }

    public override void Leave()
    {
        m_is_scene = false;
        if (null != m_dic_ens)
        {
            m_dic_ens.Clear();
        }
        if (null != m_recyle_ens)
        {
            m_recyle_ens.Clear();
        }
    }
#if _CLIENT_
    //创建实体
    void UpdateInGameCamera()
    {
        GameObject cam = GameObject.Find(EntityPredefined.SceneCamera);
        if (null == cam)
        {
            return;
        }
        InGameFollowCamera follow_cam = cam.GetComponent<InGameFollowCamera>();
        if (null == follow_cam)
        {
            return;
        }
        follow_cam.UpdateByFollow();
    }
    
#endif
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
            Logic.Instance().FrameSynLogic.ActivePlayerEn();
        }
#endif
    }

    //实体操作
    void EnOp(IEvent ev)
    {
        COpEvent coe = ev as COpEvent;
        if (null == coe || coe.IsLocal)
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


#if !_CLIENT_
    //key : frame_index, value : CRecordEventS
    Dictionary<int, CEntityEvent> m_record_evs;
    void RecordEv(IEvent ev)
    {
        CEntityEvent re = ev as CEntityEvent;
        if (null == re)
        {
            return;
        }
        m_record_evs.Add(re.FrameIndex, re);

    }
    public override Dictionary<int, CEntityEvent> GetRecordEvs()
    {
        return m_record_evs;
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
        net_mng.Send(en_id, (short)EventPredefined.MsgType.EMT_ENTITY_CHANGE_SCENE, new CEnterChangeSceneEvent());
    }

    //等待玩家入场
    List<int> m_list_player_conn_index;
    void HandleEnterInGame(IEvent ev, int en_id)
    {
        m_list_player_conn_index.Add(en_id);
        NotifyToChangeScene(en_id);
    }
    int m_ready_player_count;
    void WaitForAllClintReady(IEvent ev)
    {
        CClientReadyEvent cre = ev as CClientReadyEvent;
        if (null == cre)
        {
            return;
        }
        ++m_ready_player_count;
        if(EventPredefined.max_player == m_ready_player_count)
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
            case EventPredefined.MsgType.EMT_ENTITY_CREATE:
            case EventPredefined.MsgType.EMT_ENTITY_DESTROY:
            case EventPredefined.MsgType.EMT_ENTITY_OP:
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
            case EventPredefined.MsgType.EMT_ENTITY_CREATE:
            case EventPredefined.MsgType.EMT_ENTITY_DESTROY:
            case EventPredefined.MsgType.EMT_ENTITY_OP:
            HandleEntityEvent(ev);
            return;
            case EventPredefined.MsgType.EMT_ENTER_GAME:
            HandleEnterInGame(ev, en_id);
            break;
            case EventPredefined.MsgType.EMT_CLIENT_READY:
            WaitForAllClintReady(ev);
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
#if _CLIENT_
        //UpdateInGameCamera();
#endif
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
    public override void UpdateTankEnPostions()
    {
        foreach (var pair in m_dic_ens)
        {
            TankEntity en = pair.Value as TankEntity;
            if (null == en)
            {
                continue;
            }
            en.RecordCurPos();
            en.UpdateTargetPos();
        }
    }
}
