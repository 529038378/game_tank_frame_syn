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
            Logic.Instance().GetNetMng().Send((short) EventPredefined.MsgType.EMT_ENTITY_OP, new COpEvent(FrameIndex, m_player_en.EnId, m_player_en.GetOpType()));
        }
    }
#else
    void ServerProcess()
    {
        //给其他客户端同步实体事件
        Dictionary<int, CEntityEvent> record_evs = Logic.Instance().GetSceneMng().GetRecordEvs();
        foreach(var pair in record_evs)
        {
            Logic.Instance().GetNetMng().BroadCast(pair.Key, pair.Value, false);
        }
    }
#endif

    float acc_time = 0;
    public override void Update()
    {
        acc_time += Time.deltaTime;
        while(acc_time > NetworkPredefinedData.frame_syn_gap)
        {
            FrameIndex++;
            FrameBeginTime = Time.time;
            Logic.Instance().GetSceneMng().UpdateTankEnPostions();
#if _CLIENT_
            ClientProcess();
#else
            ServerProcess();
#endif
            acc_time -= NetworkPredefinedData.frame_syn_gap;
        }
        FrameRatio = acc_time / NetworkPredefinedData.frame_syn_gap;
    }

    IEntity m_player_en;
#if _CLIENT_
    public override void ActivePlayerEn()
    {
        m_player_en = Logic.Instance().GetOpEn();
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
    }
    public override float FrameBeginTime
    {
        get;
        set;
    }
}
