using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if _CLIENT_
public class QuitGame : MonoBehaviour
{
    public void OnClick()
    {
        Logic.Instance().QuitGame();
    }
}
#endif
