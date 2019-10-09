using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InGameUI : MonoBehaviour
{
    public GameObject m_player_obj = null;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        m_player_obj.transform.Translate(0.0f, 0.0f, 1.0f * Time.deltaTime);
    }

    private void OnGUI()
    {
        Rect op_rect = new Rect(0, UnityEngine.Screen.height - 256, 256, 256);
        GUILayout.BeginArea(op_rect, "操作区域");
        if (GUILayout.Button("向前"))
        {
            if (null != m_player_obj)
            {
                m_player_obj.transform.Translate(0.0f, 0.0f, 1.0f);
            }
        }
        GUILayout.EndArea();
    }
}
