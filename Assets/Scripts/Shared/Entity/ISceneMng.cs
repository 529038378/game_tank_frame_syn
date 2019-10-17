using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ISceneMng 
{
    public abstract bool InScene { get; }
    public abstract void Enter();
    public abstract void Leave();
    public abstract void ImplementCurFrameOpType();
    public abstract Dictionary<int, IEntity> GetSceneEns();
    public abstract string GetNonLocalEn();
    public abstract IFrameSyn FrameSynLogic { get; }
#if _CLIENT_
    public abstract void HandleEvent(IEvent ev);
    public abstract void RecycleEn(IEntity en);
    public abstract void CreateBullet(Vector3 pos, Vector3 forward);
#else
    public abstract void HandleEvent(IEvent ev, int conn_id);
    public abstract Dictionary<int, List<IEvent>> GetRecordEvs();
#endif
    public abstract void Update();
}
