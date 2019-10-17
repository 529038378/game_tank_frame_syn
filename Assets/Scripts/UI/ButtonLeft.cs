using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if _CLIENT_
public class ButtonLeft : MonoBehaviour
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
        GetOpEn().SetPreOpType(EntityPredefined.EntityOpType.EOT_LEFT, EntityPredefined.EntityExtOpType.EEOT_NONE);
    }
    public void ButtonUp()
    {
        GetOpEn().SetPreOpType(EntityPredefined.EntityOpType.EOT_IDLE, EntityPredefined.EntityExtOpType.EEOT_NONE);
    }
}
#endif