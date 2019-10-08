using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Logic : MonoBehaviour
{
    // Start is called before the first frame update
    NetManagerInterface m_network_mng = null;
    void Start()
    {
#if _CLIENT_
        m_network_mng = new ClientNetManager();
#else
        m_network_mng = new ServerNetManager();
#endif
        if (null != m_network_mng)
        {
            m_network_mng.Init();
        }
        else
        {
            Debug.LogError("fail to init network mng");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDestroy()
    {
        if (null != m_network_mng)
        {
            m_network_mng.Quit();
        }
    }
}
