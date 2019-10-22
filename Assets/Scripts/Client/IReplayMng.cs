using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class IReplayMng
{
    public abstract void Enter();
    public abstract void Leave();
    public abstract void Record(int frame_index, IEvent ev);
    public abstract void StartReplay();
    public abstract void OnAccelerate(bool accelerate);
    public abstract bool IsInReplay { get; }
    public abstract void Update();
}
