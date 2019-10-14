using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InGameState : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        if (null == Logic.Instance()
           || null == Logic.Instance().GetNetMng())
        {
            return;
        }

        Text text = GetComponent<Text>();
        //非本地实体的状态
        text.text = Logic.Instance().GetSceneMng().GetNonLocalEn();
    }
}
