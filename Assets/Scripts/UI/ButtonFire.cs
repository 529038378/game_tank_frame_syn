using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if _CLIENT_
public class ButtonFire : MonoBehaviour
{
    IEntity GetOpEn()
    {
        if (Logic.Instance())
        {
            return Logic.Instance().GetOpEn();
        }
        return null;
    }

    public void ButtonUp()
    {
        GetOpEn().Op(GetOpEn().GetOpType(), EntityPredefined.EntityExtOpType.EEOT_FIRE);
    }
}
#endif
