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
        if(null == GetOpEn())
        {
            return;
        }
        GetOpEn().Op(EntityPredefined.EntityOpType.EOT_BACKWARD, EntityPredefined.EntityExtOpType.EEOT_NONE);        
    }
    public void ButtonUp()
    {
        if (null == GetOpEn())
        {
            return;
        }
        GetOpEn().Op(EntityPredefined.EntityOpType.EOT_IDLE, EntityPredefined.EntityExtOpType.EEOT_NONE);
    }
}
#endif
