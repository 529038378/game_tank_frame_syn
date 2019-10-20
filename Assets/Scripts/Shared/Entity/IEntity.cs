using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class IEntity 
{
    public abstract void Init();
    public abstract void Update(int delta_time);
    public abstract void Destroy();
    public abstract void Op(EntityPredefined.EntityOpType op_type, EntityPredefined.EntityExtOpType ext_op_type, bool record = true);
    public abstract void ImplementCurFrameOpType();
    public abstract GameObject GetObj();
    public abstract int EnId
    {
        get; set;
    }
    public abstract EntityPredefined.EntityOpType GetOpType();
    public abstract EntityPredefined.EntityExtOpType GetExtOpType();
#if _CLIENT_
    public abstract void ResetOpType();
    public abstract void DestoryImm();
#endif
}
