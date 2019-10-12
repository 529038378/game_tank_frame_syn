using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class IEntity 
{
    public abstract void Init();
    public abstract void Update();
    public abstract void Destroy();
    public abstract void Op(EntityPredefined.EntityOpType op_type);
    public abstract void RecordCurPos();
    public abstract GameObject GetObj();
    public abstract int EnId
    {
        get; set;
    }
    public abstract EntityPredefined.EntityOpType GetOpType();
}
