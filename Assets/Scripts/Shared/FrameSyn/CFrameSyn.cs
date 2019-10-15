using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CFrameSyn : IFrameSyn
{
    public override void Enter()
    {
        FrameIndex = 0;
        acc_time = 0;
        IsWorking = true;
        FrameBeginTime = Time.time;
        FrameRatio = 0;
    }
#if _CLIENT_
    
    void ClientProcess()
    {
        //把PlayerEn的操作同步到服务器端
        if (null != m_player_en)
        {
            Logic.Instance().GetNetMng().Send((short) EventPredefined.MsgType.EMT_ENTITY_OP, new COpEvent(FrameIndex, m_player_en.EnId, m_player_en.GetOpType(), m_player_en.GetExtOpType()));
        }
    }
#else
    void ServerProcess()
    {
        //给其他客户端同步实体事件
        Dictionary<int, List<IEvent>> record_evs = Logic.Instance().GetSceneMng().GetRecordEvs();
        CSynOpEvent ev = new CSynOpEvent();
        if (record_evs.ContainsKey(Logic.Instance().FrameSynLogic.FrameIndex))
        {
            //存在对应的操作类型
            ev.FrameIndex = FrameIndex;
            ev.RecordEnEvs = record_evs[Logic.Instance().FrameSynLogic.FrameIndex];
            foreach (var ree in ev.RecordEnEvs)
            {
                COpEvent oe = ree as COpEvent;
                if (null == oe)
                {
                    continue;
                }
                if (EntityPredefined.EntityOpType.EOT_IDLE != oe.OpType)
                {
                    Debug.Log(" frame index : " + oe.FrameIndex.ToString() + " op type : " + oe.OpType.ToString());
                }
            }
           
        }
        else
        {
            //不包含的话， 表示所有客户端的帧都比服务器落后，不太可能
            List<IEvent> coll_evs = new List<IEvent>();
            foreach (var en in Logic.Instance().GetSceneMng().GetSceneEns())
            {
                CEntity cee = en.Value as CEntity;
                if (null == cee)
                {
                    continue;
                }
                coll_evs.Add(new COpEvent(FrameIndex, cee.EnId, cee.GetOpType(), cee.GetExtOpType()));
            }
            ev.FrameIndex = FrameIndex;
            ev.RecordEnEvs = coll_evs;
        }
        Logic.Instance().GetNetMng().BroadCast((short) EventPredefined.MsgType.EMT_SYN_ENTITY_OPS, ev);
    }
#endif

    float acc_time = 0;
    public override void Update()
    {
        acc_time += Time.deltaTime;
        while(acc_time > NetworkPredefinedData.frame_syn_gap)
        {
            FrameBeginTime = Time.time;
            Logic.Instance().GetSceneMng().ImplementCurFrameOpType();
            //Logic.Instance().GetSceneMng().UpdateTankEnPostions();
            //FrameIndex的顺序这样是为了保证在两端实体创建帧跟同帧的操作帧不冲突
#if _CLIENT_
            FrameIndex++;
            ClientProcess();
#else
            ServerProcess();
            FrameIndex++;
#endif
            acc_time -= NetworkPredefinedData.frame_syn_gap;
        }
        FrameRatio = acc_time / NetworkPredefinedData.frame_syn_gap;
    }

    IEntity m_player_en;
#if _CLIENT_
    public override void ActivePlayerEn(int frame_begin_index)
    {
        m_player_en = Logic.Instance().GetOpEn();
        FrameIndex = frame_begin_index;
    }
    
#endif

    public override int FrameIndex
    {
        get;set;
    }
    public override float FrameRatio { get; set; }

    public override bool IsWorking { get; set; }

    public override void Leave()
    {
        IsWorking = false;
        FrameIndex = 0;
        acc_time = 0;
        FrameBeginTime = 0;
        FrameRatio = 0;
    }

    public override float FrameBeginTime
    {
        get;
        set;
    }
}
