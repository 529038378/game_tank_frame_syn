﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Linq;

public class CSceneMng : ISceneMng, INetManagerCallback
#if _CLIENT_
    , IColliderCallback
#endif
{
    public CSceneMng()
    {
#if !_CLIENT_
        m_list_player_conn_index = new List<int>();
        m_record_evs = new Dictionary<int, Dictionary<int, IEvent>>();
#else
        m_dic_evs = new Dictionary<int, List<IEvent>>();
        m_frameindex_pre_snapshot = new Dictionary<int, List<EntityPredefined.EntityPreSnapShot>>();
        m_frameindex_snapshot = new Dictionary<int, List<EntityPredefined.EntitySnapShot>>();
#endif
        m_dic_ens = new Dictionary<int, IEntity>();
        m_recyle_ens = new Dictionary<int, IEntity>();
        m_collider_en_map = new Dictionary<Collider, IEntity>();
        m_dic_bullet_ens = new Dictionary<int, IEntity>();
        m_frame_syn = new CFrameSyn();


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
    //int - en_hashcode, IEntity - concreta_bullet_en
    Dictionary<int, IEntity> m_dic_bullet_ens;
    //int - en_id, IEntity - recycle_en
    Dictionary<int, IEntity> m_recyle_ens;


    public override void Enter()
    {
        m_is_scene = true;
        m_dic_ens.Clear();
        m_recyle_ens.Clear();
        m_frame_syn.Enter();

#if _CLIENT_
        if (!Logic.Instance().GetReplayMng().IsInReplay)
        {
            Logic.Instance().NotifyClientReady();
            Logic.Instance().GetReplayMng().Enter();
        }
        m_acc_time = 0;
        m_acc_time_in_one_logic_frame = 0;
        m_collider_en_map.Clear();
        m_dic_bullet_ens.Clear();
        m_need_rollback = false;
#else
        m_ready_player_count = 0;
        m_record_evs.Clear();
#endif
    }

    public override void Leave()
    {
        m_is_scene = false;
        if (null != m_dic_ens)
        {
            foreach (var pair in m_dic_ens)
            {
                if (null != pair.Value)
                {
#if _CLIENT_
                    pair.Value.DestoryImm();
#endif
                }
            }
            m_dic_ens.Clear();
        }
        if (null != m_recyle_ens)
        {
            m_recyle_ens.Clear();
        }
        if (null != Logic.Instance()
            && null != Logic.Instance().FrameSynLogic)
        {
            Logic.Instance().FrameSynLogic.Leave();
        }
#if _CLIENT_
        m_acc_time = 0;
        m_collider_en_map.Clear();
        m_dic_bullet_ens.Clear();
        m_acc_time_in_one_logic_frame = 0;
        m_frameindex_pre_snapshot.Clear();
        m_need_rollback = false;
        if(Logic.Instance().GetReplayMng().IsInReplay)
        {
            Logic.Instance().GetReplayMng().Leave();
        }
#else
        m_ready_player_count = 0;
        m_record_evs.Clear();
        m_list_player_conn_index.Clear();
#endif

    }
    public override Dictionary<int, IEntity> GetSceneEns()
    {
        return m_dic_ens;
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
    int m_local_en_id;
    void AddToColliderMap(IEntity en)
    {
        if (null == en.GetObj())
        {
            return;
        }
        Collider coll = en.GetObj().GetComponent<Collider>();
        if (null == coll)
        {
            return;
        }
        m_collider_en_map.Add(coll, en);
    }
    public override void UpdateReplay(bool accelerate)
    {
        if (m_just_enter_new_logic_frame)
        {
            CheckLastFrameLeftMovePos();
            m_acc_time_in_one_logic_frame = 0;
        }
        if (accelerate)
        {
            AccelerateUpdate();
            m_acc_time_in_one_logic_frame = NetworkPredefinedData.frame_syn_gap;
        }
        else
        {
            NormalUpdate();
        }
        m_just_enter_new_logic_frame = false;
        if (FrameSynLogic.Update(false))
        {
            m_just_enter_new_logic_frame = true;
        }
    }
#endif
    //碰撞检测维护的表
    Dictionary<Collider, IEntity> m_collider_en_map;
    void CreateEn(IEvent ev)
    {
        CCreateEvent cce = ev as CCreateEvent;
        if (null == cce)
        {
            return;
        }
        IEntity en = null;
        switch (cce.EnType)
        {
            case EntityPredefined.EntityType.EET_TANK:
            en = new TankEntity(cce.EnId, cce.IsLocal, cce.CampType, cce.SpwanPosIndex);
            break;
        }
        m_dic_ens.Add(cce.EnId, en);
#if _CLIENT_
        AddToColliderMap(en);
        if (cce.IsLocal)
        {
            Logic.Instance().SetOpEn(en);
            Logic.Instance().FrameSynLogic.ActivePlayerEn(cce.FrameIndex);
            m_local_en_id = cce.EnId;
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
        en.Op(coe.OpType, coe.OpExtType, false);
    }

    //摧毁实体
    void DestoryOp(IEvent ev)
    {
#if !_CLIENT_
        return;
#endif
        CDestoryEvent cde = ev as CDestoryEvent;
        if (null == cde)
        {
            return;
        }
        if (!m_dic_ens.ContainsKey(cde.EnId))
        {
            return;
        }
        IEntity en = m_dic_ens[cde.EnId];
        m_dic_ens.Remove(cde.EnId);
#if !_CLIENT_
        --m_ready_player_count;
        en.Destroy();
#else
        en.DestoryImm();
#endif
    }


#if !_CLIENT_
    //key : frame_index, value : (key :  en_id , value : CRecordEventS)
    volatile Dictionary<int, Dictionary<int, IEvent>> m_record_evs;
    bool NeedReplaceEv(IEvent old_ev, IEvent new_ev)
    {
        bool res = true;
        if((short)EventPredefined.EntityEventType.ET_DESTROY == (short)new_ev.GetEventType())
        {
            res = true;
        }
        else if ((short)EventPredefined.EntityEventType.ET_DESTROY == (short)old_ev.GetEventType())
        {
            res = false;
        }
        
        return res;
    }
    void RecordEv(IEvent ev)
    {
        CEntityEvent re = ev as CEntityEvent;
        if (null == re)
        {
            return;
        }
        lock(RecordEvsLock)
        {
            if (m_record_evs.ContainsKey(re.FrameIndex))
            {
                Dictionary<int,IEvent> en_dic = m_record_evs[re.FrameIndex];
                if (en_dic.ContainsKey(re.EnId) && NeedReplaceEv(en_dic[re.EnId], ev))
                {
                    en_dic[re.EnId] = re;
                }
                else
                {
                    en_dic.Add(re.EnId, re);
                }
            }
            else
            {
                Dictionary<int, IEvent> dic_ev = new Dictionary<int, IEvent>();
                dic_ev.Add(re.EnId, re);
                m_record_evs.Add(re.FrameIndex, dic_ev);
            }
        }
        COpEvent coe = re as COpEvent;
        if(null != coe)
        {
            Debug.Log(" client frame index : " + coe.FrameIndex.ToString()
            + " op type : " + coe.OpType.ToString());
        }
        
        m_record_evs = m_record_evs.OrderBy(o => o.Key).ToDictionary(o=>o.Key, p=>p.Value);
    }
    public override Dictionary<int, Dictionary<int,IEvent>> GetRecordEvs()
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
            case EventPredefined.EntityEventType.ET_DESTROY:
            DestoryOp(ev);
            break;
            case EventPredefined.EntityEventType.ET_OP:
            EnOp(ev);
            break;
            default:
            Debug.Assert(false);
            break;
        }
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
            CEntityEvent ev = new CCreateEvent(Logic.Instance().FrameSynLogic.FrameIndex, conn_id, EntityPredefined.EntityType.EET_TANK, EntityPredefined.EntityCampType.ECT_PLAYER, index);
            net_mng.BroadCast(conn_id, ev, false);
            index++;
            RecordEv(ev);
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
    void PlayerLeaveInGame(IEvent ev, int en_id)
    {
        CClientLeaveInGameEvent cclige = ev as CClientLeaveInGameEvent;
        if(null == cclige)
        {
            return;
        }
        if (m_dic_ens.ContainsKey(en_id))
        {
            m_dic_ens.Remove(en_id);
        }
        if(0 == m_dic_ens.Count)
        {
            Logic.Instance().LeaveGame();
        }
    }
#endif

#if _CLIENT_
    //处理网络事件
    void ChangeToInGameScene()
    {
        Logic.Instance().EnterInGame();
    }
    bool m_need_rollback = false;
    void HandleFrameSynOpsEv(IEvent ev)
    {
        CSynOpEvent soe = ev as CSynOpEvent;
        if (null == soe)
        {
            return;
        }

        m_need_rollback = CheckOpEvent(ev);
        if(!m_need_rollback)
        {
            //同步其他的实体操作
            foreach (var pair in soe.RecordEnEvs)
            {
                IEvent ee = pair as IEvent;
                HandleEntityEvent(ee);
            }
        }
        
        
        //Debug.Log(" server frame index : " + soe.FrameIndex.ToString() + ", local frame index : " + Logic.Instance().FrameSynLogic.FrameIndex.ToString());
    }
    void RecordToReplay(IEvent ev)
    {
        if (Logic.Instance().GetReplayMng().IsInReplay)
        {
            return;
        }

        CEntityEvent cee = ev as CEntityEvent;
        if (null == cee)
        {
            return;
        }

        Logic.Instance().GetReplayMng().Record(cee.FrameIndex, cee);
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
            case EventPredefined.MsgType.EMT_SYN_ENTITY_OPS:
            HandleFrameSynOpsEv(ev);
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
            case EventPredefined.MsgType.EMT_CLIENT_LEAVE_INGAME:
            PlayerLeaveInGame(ev, en_id);
            break;
            default:
            Debug.Log("wrong event type");
            Debug.Assert(false);
            break;
        }
    }
#endif
    int m_acc_time;
    int m_acc_time_in_one_logic_frame;
    IFrameSyn m_frame_syn;
    void StepUpdateEnRender()
    {
        foreach (var pair in m_dic_ens)
        {
            IEntity en = pair.Value as IEntity;
            if (null == en)
            {
                continue;
            }
            en.Update(EntityPredefined.render_update_gap);
        }

        foreach (var pair in m_dic_bullet_ens)
        {
            IEntity en = pair.Value as IEntity;
            if (null == en)
            {
                continue;
            }
            en.Update(EntityPredefined.render_update_gap);
        }

        foreach (var pair in m_recyle_ens)
        {
            IEntity en = pair.Value as IEntity;
            if (null == en)
            {
                continue;
            }
            en.Destroy();
            if (m_dic_ens.ContainsKey(en.EnId))
            {
                m_dic_ens.Remove(en.EnId);
            }
            else if (m_dic_bullet_ens.ContainsKey(en.EnId))
            {
                m_dic_bullet_ens.Remove(en.EnId);
            }
        }
        m_recyle_ens.Clear();
        m_acc_time_in_one_logic_frame += EntityPredefined.render_update_gap;
    }
#if _CLIENT_
    public bool NeedAccelerate { get; set; }
    int ProcessOneLogicFrameEv()
    {
        IDictionaryEnumerator itr = m_dic_evs.GetEnumerator();
        if(!itr.MoveNext())
        {
            return -1;
        }
        List<IEvent> list_evs = itr.Value as List<IEvent>;
        foreach(var ev in list_evs)
        {
            HandleEvent(ev);
        }
        int key =(int) itr.Key;
        m_dic_evs.Remove(key);
        //Debug.Log(" frame index change : " + key.ToString());
        return key;
    }

    void AcceOneLogicFrameRender()
    {
        foreach (var pair in m_dic_ens)
        {
            IEntity en = pair.Value as IEntity;
            if (null == en)
            {
                continue;
            }
            en.Update(NetworkPredefinedData.frame_syn_gap);
        }

        foreach (var pair in m_dic_bullet_ens)
        {
            IEntity en = pair.Value as IEntity;
            if (null == en)
            {
                continue;
            }
            en.Update(NetworkPredefinedData.frame_syn_gap);
        }

        foreach (var pair in m_recyle_ens)
        {
            IEntity en = pair.Value as IEntity;
            if (null == en)
            {
                continue;
            }
            en.Destroy();
            if (m_dic_ens.ContainsKey(en.EnId))
            {
                m_dic_ens.Remove(en.EnId);
            }
            else if (m_dic_bullet_ens.ContainsKey(en.EnId))
            {
                m_dic_bullet_ens.Remove(en.EnId);
            }
        }
        m_recyle_ens.Clear();
        m_acc_time_in_one_logic_frame += NetworkPredefinedData.frame_syn_gap;
        return;
        float acc_render_time = NetworkPredefinedData.frame_syn_gap;
        while(acc_render_time - EntityPredefined.render_update_gap > -0.000001)
        {
            StepUpdateEnRender();
            acc_render_time -= EntityPredefined.render_update_gap;
        }
    }
    
    void CheckLastFrameLeftMovePos()
    {
        while(NetworkPredefinedData.frame_syn_gap > m_acc_time_in_one_logic_frame)
        {
            StepUpdateEnRender();
            m_acc_time_in_one_logic_frame += EntityPredefined.render_update_gap;
        }
    }

    void NormalUpdate()
    {
        if (m_just_enter_new_logic_frame)
        {
            int frame_index = ProcessOneLogicFrameEv();
            //没有收到服务器的同步消息，已经开始预测活动了,并记录要回退的位置节点信息
            if (-1 == frame_index)
            {
                RecordSnapShot();
                RecordPreSnapShot();
            }
            ImplementCurFrameOpType();
            m_acc_time = FrameSynLogic.FrameBeginAccTime;
        }
        m_acc_time +=(int) (Time.deltaTime * 1000);
        while ((m_acc_time - EntityPredefined.render_update_gap > 0.000001) && (NetworkPredefinedData.frame_syn_gap > m_acc_time_in_one_logic_frame))
        {
            StepUpdateEnRender();
            m_acc_time -= EntityPredefined.render_update_gap;
        }
    }
    AccUpdateRes AccelerateUpdate()
    {
        lock(m_dic_evs_lock)
        {
            AccUpdateRes res = new AccUpdateRes();
            int left_times = 0;// EntityPredefined.AccTime;
            while (left_times < m_dic_evs.Count && left_times < EntityPredefined.AccTime)
            {
                res.m_cur_acc_frame = ProcessOneLogicFrameEv();
                if (-1 == res.m_cur_acc_frame)
                {
                    break;
                }
                if (m_need_rollback)
                {
                    ResetToSnapShot();
                }
                ImplementCurFrameOpType();
                AcceOneLogicFrameRender();
                ++left_times;
            }
            res.m_left_frame_acc_count = m_dic_evs.Count;
            return res;
        }
    }
    //记录按照预测行动的快照
    Dictionary<int, List<EntityPredefined.EntityPreSnapShot>> m_frameindex_pre_snapshot;//只记录第一个
    void RecordPreSnapShot()
    {
        if (0 != m_frameindex_pre_snapshot.Count)
        {
            return;
        }
        List<EntityPredefined.EntityPreSnapShot> list = new List<EntityPredefined.EntityPreSnapShot>();
        foreach (var en in m_dic_ens)
        {
            TankEntity te = en.Value as TankEntity;
            if (null == te)
            {
                continue;
            }
            EntityPredefined.EntityPreSnapShot ss = new EntityPredefined.EntityPreSnapShot();
            ss.EnId = te.EnId;
            ss.OpType = te.GetEntityOpType();
            ss.ExtOpType = te.GetEntityExtOpType();
            list.Add(ss);
            Debug.Log(" Record PreSnapShot : " + FrameSynLogic.FrameIndex.ToString());
        }
        m_frameindex_pre_snapshot.Add(FrameSynLogic.FrameIndex, list);
    }

    //记录一帧开始前的快照
    Dictionary<int, List<EntityPredefined.EntitySnapShot>> m_frameindex_snapshot;//其实只有一个pair，index只记录上一次最后的一次快照
    bool IsInPreMove()
    {
        return m_frameindex_pre_snapshot.Count >= 1;
    }
    void RecordSnapShot()
    {
        //已经进行预测了就不要记录位置快照了
        if(IsInPreMove())
        {
            return;
        }
        m_frameindex_snapshot.Clear();
        List<EntityPredefined.EntitySnapShot> list = new List<EntityPredefined.EntitySnapShot>();
        foreach (var en in m_dic_ens)
        {
            TankEntity te = en.Value as TankEntity;
            if (null == te)
            {
                continue;
            }
            EntityPredefined.EntitySnapShot ss = new EntityPredefined.EntitySnapShot();
            ss.EnId = te.EnId;
            ss.OpType = te.GetEntityOpType();
            ss.ExtOpType = te.GetEntityExtOpType();
            ss.Pos = te.GetObj().transform.position;
            list.Add(ss);
            Debug.Log("\n en id : " + ss.EnId.ToString()
                + " pos : " + ss.Pos.ToString());
        }
        m_frameindex_snapshot.Add(FrameSynLogic.FrameIndex, list);
    }
    void ResetToSnapShot()
    {
        if(m_frameindex_snapshot.Count <= 0)
        {
            return;
        }
        IDictionaryEnumerator itr = m_frameindex_snapshot.GetEnumerator();
        if(itr.MoveNext())
        {
            List<EntityPredefined.EntitySnapShot> list_evs = itr.Value as List<EntityPredefined.EntitySnapShot>;
            foreach(var ev in list_evs)
            {
                if (m_dic_ens.ContainsKey(ev.EnId))
                {
                    TankEntity te = m_dic_ens[ev.EnId] as TankEntity;
                    if (null == te)
                    {
                        continue;
                    }
                    te.GetObj().transform.position = ev.Pos;
                    te.Op(ev.OpType, ev.ExtOpType, false);
                    Debug.Log(" Return to SnapShot : frame index : " + itr.Key.ToString()
                        + " en id : " + te.EnId.ToString()
                        + " en pos : " + ev.Pos.ToString());

                }
            }
        }
        ImplementCurFrameOpType();
        //预测记录清空
        m_frameindex_pre_snapshot.Clear();
        int key = (int)itr.Key;
        FrameSynLogic.FrameIndex = key;
        m_need_rollback = false;
    }
    bool CheckOpEvent(IEvent ev)
    {
        CEntityEvent cee = ev as CEntityEvent;
        if(null == cee)
        {
            return false;
        }
        if(m_frameindex_pre_snapshot.ContainsKey(cee.FrameIndex))
        {
            //需要回滚的状态
            return true;
        }
        return false;
    }
#else
    object RecordEvsLock = new object();
    void NormalUpdateServer()
    {
        if (!m_just_enter_new_logic_frame)
        {
            return;
        }
        if (m_record_evs.ContainsKey(FrameSynLogic.FrameIndex))
        {
            lock(RecordEvsLock)
            {
                Dictionary<int, IEvent> dic_evs = m_record_evs[FrameSynLogic.FrameIndex];
                foreach (var ev in dic_evs)
                {
                    HandleEntityEvent(ev.Value);
                    ImplementCurFrameOpType();
                }
            }

//             m_acc_time +=(int) (Time.deltaTime * 1000);
//             while (m_acc_time > EntityPredefined.render_update_gap)
//             {
//                 StepUpdateEnRender();
//                 m_acc_time -= EntityPredefined.render_update_gap;
//             }
        }
    }
    public bool AddEntityEv(IEvent ev)
    {
        CEntityEvent cee = ev as CEntityEvent;
        if(null == cee)
        {
            return false;
        }
        RecordEv(ev);
        return true;
    }
#endif
    bool m_just_enter_new_logic_frame = false;
    struct AccUpdateRes
    {
        public int m_cur_acc_frame;
        public int m_left_frame_acc_count;
    }
    public override void Update()
    {
#if _CLIENT_
        if(m_just_enter_new_logic_frame)
        {
            if (m_need_rollback)
            {
                ResetToSnapShot();
            }
            else
            {
                CheckLastFrameLeftMovePos();
            }
            m_acc_time_in_one_logic_frame = 0;
        }

        if (NeedAccelerate)
        {
            AccUpdateRes acc_update_res = AccelerateUpdate();
            NeedAccelerate = acc_update_res.m_left_frame_acc_count > 1;
            if (!NeedAccelerate && -1 != acc_update_res.m_cur_acc_frame)
            {
                //加速完毕，重新对齐一下frameindex
                FrameSynLogic.FrameIndex = acc_update_res.m_cur_acc_frame + NetworkPredefinedData.frame_client_syn_pre_offset;
            }
            m_acc_time_in_one_logic_frame = NetworkPredefinedData.frame_syn_gap;
        }
        else
        {
            NormalUpdate();
        }
#else
        NormalUpdateServer();
#endif

        //在每一帧处理完了之后加个快照，每次ProcessOneLogicFrameEv的时候判断一下当前帧的操作跟之前预测的操作是否一样，不一样的话，需要1、读取快照2、重置到快照的位置3、然后进行后续的ProcessOneLogicFrameEv和StepUpdateEnRender等操作。然后记录快照，快照的操作和预测的操作都只用维护一帧的数据就行，就是处理上一次服务器发过来的数据处理之后的状态，可以共用一个结构。
        m_just_enter_new_logic_frame = false;
        if (null != m_frame_syn && m_frame_syn.IsWorking)
        {
            if(m_frame_syn.Update()
#if _CLIENT_
                //&& !NeedAccelerate
#endif
                )
            {
#if _CLIENT_
                //进入新的逻辑帧之后的一些处理
                if (m_dic_evs.Count > 1)
                {
                    NeedAccelerate = true;
                }
#endif
                m_just_enter_new_logic_frame = true;
            }
        }
    }
    public override void ImplementCurFrameOpType()
    {
        foreach (var pair in m_dic_ens)
        {
            TankEntity en = pair.Value as TankEntity;
            if (null == en)
            {
                continue;
            }
            en.ImplementCurFrameOpType();
        }
    }
//     public override void UpdateTankEnPostions()
//     {
//         foreach (var pair in m_dic_ens)
//         {
//             TankEntity en = pair.Value as TankEntity;
//             if (null == en)
//             {
//                 continue;
//             }
//             en.RecordCurPos();
//             en.UpdateTargetPos();
//         }
//     }
    public override string GetNonLocalEn()
    {
#if _CLIENT_
        string res = "Cur FrameIndex :" + FrameSynLogic.FrameIndex.ToString() + "\nEntity State : \n";
        foreach(var en in m_dic_ens)
        {
            TankEntity cen = en.Value as TankEntity;
            if (null != cen)
            {
                res += "FrameIndex : " + cen.MovStateSeqFrameNum.ToString() + " en id : " + cen.EnId.ToString() + " stay pos : " + cen.GetObj().transform.position.ToString() + "\n";
            }
        }
        if (NeedAccelerate)
        {
            res += "加速同步中……";
        }
        else
        {
            res += "正常同步中……";
        }
        return res;
#else
        return "";
#endif
    }
#if _CLIENT_
    public void OnCollision(Collider coll)
    {
        if(!m_collider_en_map.ContainsKey(coll))
        {
            return;
        }

        IEntity en = m_collider_en_map[coll];
        if (null != en)
        {
            Logic.Instance().GetNetMng().Send((short) EventPredefined.MsgType.EMT_ENTITY_DESTROY, new CDestoryEvent(Logic.Instance().FrameSynLogic.FrameIndex, en.EnId));
        }
    }
    public override void RecycleEn(IEntity en)
    {
        m_recyle_ens.Add(en.EnId, en);
    }
    
    public override void CreateBullet(Vector3 pos, Vector3 forward)
    {
        Vector3 bullet_swpan_pos = pos + forward.normalized * EntityPredefined.BulletSwpanOffset.z;
        bullet_swpan_pos.y += EntityPredefined.BulletSwpanOffset.y;
        IEntity en = new BulletEntity(bullet_swpan_pos, forward);
        m_dic_bullet_ens.Add(en.GetHashCode(), en);
    }
    object m_dic_evs_lock = new object();
    volatile Dictionary<int, List<IEvent>> m_dic_evs;
    public bool AddEntityEv(IEvent ev)
    {
        lock(m_dic_evs_lock)
        {
            CEntityEvent cee = ev as CEntityEvent;
            if (null == cee)
            {
                return false;
            }
            CSynOpEvent csoe = cee as CSynOpEvent;
            if (m_dic_evs.ContainsKey(cee.FrameIndex))
            {
                m_dic_evs[cee.FrameIndex].Add(cee);
            }
            else
            {
                List<IEvent> list_evs = new List<IEvent>();
                list_evs.Add(ev);
                m_dic_evs.Add(cee.FrameIndex, list_evs);
            }
            m_dic_evs = m_dic_evs.OrderBy(o => o.Key).ToDictionary(o => o.Key, p => p.Value);
            if (!FrameSynLogic.IsWorking)
            {
                FrameSynLogic.IsWorking = true;
                m_just_enter_new_logic_frame = true;
            }
            RecordToReplay(ev);
            return true;
        }
       
    }
   
#endif
    public override IFrameSyn FrameSynLogic
    {
        get
        {
            return m_frame_syn;
        }
    }
}
