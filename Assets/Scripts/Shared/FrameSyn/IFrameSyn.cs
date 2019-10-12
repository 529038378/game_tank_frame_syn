using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class IFrameSyn 
{
    public abstract void Update();
    public abstract int FrameIndex { get; set; }
    public abstract void Enter();
    public abstract void Leave();
    public abstract float FrameRatio { get; set; }
    public abstract bool IsWorking { get; set; }
#if _CLIENT_
    public abstract void ActivePlayerEn();
    public abstract float FrameBeginTime{get;set;}
#endif 
}
