using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonRight : MonoBehaviour
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
        GetOpEn().Op(EntityPredefined.EntityOpType.EOT_RIGHT);
    }
    public void ButtonUp()
    {
        GetOpEn().Op(EntityPredefined.EntityOpType.EOT_IDLE);
    }
}
