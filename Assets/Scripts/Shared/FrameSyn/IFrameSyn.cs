using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class IFrameSyn 
{
    public abstract bool Update(bool syns_to_server = false);
    public abstract int FrameIndex { get; set; }
    public abstract void Enter();
    public abstract void Leave();
    public abstract float FrameRatio { get; set; }
    public abstract bool IsWorking { get; set; }
    public abstract int FrameBeginAccTime { get; set; }
#if _CLIENT_
    public abstract void ActivePlayerEn(int frame_begin_index);
#endif 
}
