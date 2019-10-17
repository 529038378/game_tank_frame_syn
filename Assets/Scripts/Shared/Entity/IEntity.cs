using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class IEntity 
{
    public abstract void Init();
    public abstract void Update(float delta_time);
    public abstract void Destroy();
    public abstract void SetPreOpType(EntityPredefined.EntityOpType op_type, EntityPredefined.EntityExtOpType ext_op_type);
    public abstract void ImplementCurFrameOpType();
    public abstract GameObject GetObj();
    public abstract void Op(EntityPredefined.EntityOpType op_type, EntityPredefined.EntityExtOpType ext_op_type);
    public abstract int EnId
    {
        get; set;
    }
    public abstract EntityPredefined.EntityOpType GetOpType();
    public abstract EntityPredefined.EntityExtOpType GetExtOpType();
}
