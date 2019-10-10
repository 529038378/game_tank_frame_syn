using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ISceneMng 
{
    public abstract bool InScene { get; }
    public abstract void Enter();
    public abstract void Leave();
#if _CLIENT_
    public abstract void HandleEvent(IEvent ev);
#else
    public abstract void HandleEvent(IEvent ev, int conn_id);
#endif
    public abstract void Update();
}
