using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NetState : MonoBehaviour
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
        text.text = "当前连接数" + Logic.Instance().GetNetMng().GetConnState().ToString();
    }
}
