using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnterGame : MonoBehaviour
{
    private void Update()
    {
#if _CLIENT_
        string str = "进入游戏";
#else
        string str =  Logic.Instance().FrameSynLogic.StopFrameSyn ? "未同步" : "同步中";
#endif
        Text tex = GetComponentInChildren<Text>();
        if (null != tex)
        {
            tex.text = str;
        }
    }
#if _CLIENT_
    public void OnClick()
    {
        Logic.Instance().RequestEnterGame();
    }
#else
    public void OnClick()
    {
        Logic.Instance().SwitchServerFrameSyn();
    }
#endif

    }

