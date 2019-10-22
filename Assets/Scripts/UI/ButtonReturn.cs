using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonReturn : MonoBehaviour
{
    public void ButtonDown()
    {
        Logic.Instance().LeaveGame();
    }
}
