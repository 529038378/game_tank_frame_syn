using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonReplay : MonoBehaviour
{
    public void ButtonDown()
    {
        Logic.Instance().GetReplayMng().StartReplay();
    }
}
