using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CReplayMng : IReplayMng
{
    public CReplayMng()
    {
        m_record_evs = new Dictionary<int, List<IEvent>>();
    }

    //记录整场的事件
    Dictionary<int, List<IEvent>> m_record_evs;
    public override void Enter()
    {
        m_record_evs.Clear();
        m_is_in_replay = false;
        m_in_accelerate = false;
    }

    public override void Leave()
    {
        
    }
    public override void OnAccelerate(bool accelerate)
    {
        m_in_accelerate = accelerate;
    }
    public override void Record(int frame_index, IEvent ev)
    {
        if (m_record_evs.ContainsKey(frame_index))
        {
            Debug.Log(" exist? ");
            return;
        }
        List<IEvent> list_evs = new List<IEvent>();
        list_evs.Add(ev);
        m_record_evs.Add(frame_index, list_evs);
    }
    bool m_is_in_replay;
    bool m_in_accelerate;
    public override void StartReplay()
    {
        CSceneMng scene_mng = Logic.Instance().GetSceneMng() as CSceneMng;
        if (null == scene_mng)
        {
            return;
        }

        if (m_record_evs.Count <= 0)
        {
            return;
        }
        m_is_in_replay = true;
        //切场景
        Logic.Instance().EnterInGame();
        //把记录的事件塞给SceneMng
        foreach(var list_evs in m_record_evs)
        {
            foreach (var ev in list_evs.Value)
            {
                scene_mng.AddEntityEv(ev);
            }
        }
    }

    public override bool IsInReplay
    {
        get
        {
            return m_is_in_replay;
        }
    }
    public override void Update()
    {
#if _CLIENT_
        Logic.Instance().GetSceneMng().UpdateReplay(m_in_accelerate); 
#endif
    }
}
