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

    public void ButtonDown()
    {

    }
    public void ButtonUp()
    {
        GetOpEn().SetPreOpType(GetOpEn().GetOpType(), EntityPredefined.EntityExtOpType.EEOT_FIRE);
    }
}
#endif