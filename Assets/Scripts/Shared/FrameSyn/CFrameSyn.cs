using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CFrameSyn : IFrameSyn
{
    public override void Enter()
    {
        FrameIndex = 0;
        acc_time = 0;
        FrameBeginAccTime = 0;
        FrameRatio = 0;
#if !_CLIENT_
        IsWorking = true;
        StopFrameSyn = false;
        m_has_syn_frame_index = 0;
#endif
    }
#if _CLIENT_
    void ClientProcess()
    {
        //把PlayerEn的操作同步到服务器端
        if (null != m_player_en)
        {
             Debug.Log(" sysn frame index : " + FrameIndex.ToString()
                 + " en id : " + m_player_en.EnId.ToString()
                 + " op type : " + m_player_en.GetOpType().ToString()
                 + " ext op type : " + m_player_en.GetExtOpType().ToString());
            Logic.Instance().GetNetMng().Send((short) EventPredefined.MsgType.EMT_ENTITY_OP, new COpEvent(FrameIndex, m_player_en.EnId, m_player_en.GetOpType(), m_player_en.GetExtOpType()));
            m_player_en.ResetOpType();
        }
    }
#else
    public override bool StopFrameSyn { get; set; }
    int m_has_syn_frame_index;
    void ServerProcess()
    {
        if(StopFrameSyn)
        {
            return;
        }
        //给其他客户端同步实体事件
        while(m_has_syn_frame_index < FrameIndex)
        {
            Dictionary<int, Dictionary<int, IEvent>> record_evs = Logic.Instance().GetSceneMng().GetRecordEvs();
            CSynOpEvent ev = new CSynOpEvent();
            if (record_evs.ContainsKey(m_has_syn_frame_index))
            {
                Dictionary<int, IEvent> dic_evs = record_evs[m_has_syn_frame_index];
                //存在对应的操作类型
                ev.FrameIndex = m_has_syn_frame_index;
                foreach (var en in Logic.Instance().GetSceneMng().GetSceneEns())
                {
                    int id = (int) en.Key;
                    if (dic_evs.ContainsKey(id))
                    {
                        ev.RecordEnEvs.Add(dic_evs[id]);
                    }
                    else
                    {
                        IEntity cen = en.Value as IEntity;
                        IEvent op_ev = new COpEvent(m_has_syn_frame_index, id, cen.GetOpType(), cen.GetExtOpType());
                        ev.RecordEnEvs.Add(op_ev);
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
                    coll_evs.Add(new COpEvent(m_has_syn_frame_index, cee.EnId, cee.GetOpType(), cee.GetExtOpType()));
                }
                ev.FrameIndex = m_has_syn_frame_index;
                ev.RecordEnEvs = coll_evs;
            }
            Logic.Instance().GetNetMng().BroadCast((short) EventPredefined.MsgType.EMT_SYN_ENTITY_OPS, ev);
            foreach (var ree in ev.RecordEnEvs)
            {
                COpEvent cee = ree as COpEvent;
                Debug.Log(" frame index : " + cee.FrameIndex.ToString() + " op type : " + cee.OpType.ToString());
            }
            ++m_has_syn_frame_index;
        }
        
        m_has_syn_frame_index = FrameIndex;
    }
#endif

    int acc_time = 0;
    public override bool Update(bool syns_to_server = true)
    {
        bool enter_new_logic_frame = false;
        acc_time +=(int) (Time.deltaTime * 1000);
//         Debug.Log(" frame index :  " + FrameIndex.ToString()
//             + " delta time : " + Time.deltaTime.ToString()
//             + " acc_time : " + acc_time.ToString());
        while(acc_time > NetworkPredefinedData.frame_syn_gap)
        {
            //Logic.Instance().GetSceneMng().UpdateTankEnPostions();
            //FrameIndex的顺序这样是为了保证在两端实体创建帧跟同帧的操作帧不冲突
#if _CLIENT_
            FrameIndex++;
            if (syns_to_server)
            {
                ClientProcess();
            }
#else
            ServerProcess();
            FrameIndex++;
#endif
            acc_time -= NetworkPredefinedData.frame_syn_gap;
            enter_new_logic_frame = true;

        }
        FrameBeginAccTime = acc_time;
        FrameRatio = acc_time * 1.0f / NetworkPredefinedData.frame_syn_gap;
        return enter_new_logic_frame;
    }

    IEntity m_player_en;
#if _CLIENT_
    public override void ActivePlayerEn(int frame_begin_index)
    {
        m_player_en = Logic.Instance().GetOpEn();
        FrameIndex = frame_begin_index+NetworkPredefinedData.frame_client_syn_pre_offset;
        IsWorking = true;
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
        FrameBeginAccTime = 0;
        FrameRatio = 0;
    }

    public override int FrameBeginAccTime
    {
        get;
        set;
    }
}
