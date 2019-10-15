using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if _CLIENT_
public class ButtonBackward : MonoBehaviour
{
    IEntity GetOpEn()
    {
        if (Logic.Instance())
        {
            return Logic.Instance().GetOpEn();
        }
        return null;
    }

    public void ButtonDown()
    {
        GetOpEn().Op(EntityPredefined.EntityOpType.EOT_BACKWARD, EntityPredefined.EntityExtOpType.EEOT_NONE);        
    }
    public void ButtonUp()
    {
        GetOpEn().Op(EntityPredefined.EntityOpType.EOT_IDLE, EntityPredefined.EntityExtOpType.EEOT_NONE);
    }
}
#endif
