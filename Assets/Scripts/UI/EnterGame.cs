using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if _CLIENT_
public class EnterGame : MonoBehaviour
{
    public void OnClick()
    {
        Logic.Instance().RequestEnterGame();
    }
}
#endif
