using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ISceneMng 
{
    public abstract bool InScene { get; }
    public abstract void Enter();
    public abstract void Leave();
    public abstract void UpdateTankEnPostions();
    public abstract Dictionary<int, IEntity> GetSceneEns();
    public abstract string GetNonLocalEn();
#if _CLIENT_
    public abstract void HandleEvent(IEvent ev);
#else
    public abstract void HandleEvent(IEvent ev, int conn_id);
    public abstract Dictionary<int, List<IEvent>> GetRecordEvs();
#endif
    public abstract void Update();
}
