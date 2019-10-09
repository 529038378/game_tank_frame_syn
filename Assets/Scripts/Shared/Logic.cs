using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Logic : MonoBehaviour
{
    //测试代码
    IEntity m_op_en = null;
    public IEntity GetOpEn()
    {
        if (null == m_op_en)
        {
            m_op_en = new TankEntity();
        }
        return m_op_en;
    }

    static Logic m_instance = null;
    public static Logic Instance()
    {
        return m_instance;
    }

    // Start is called before the first frame update
    NetManagerInterface m_network_mng = null;
    void Start()
    {
        DontDestroyOnLoad(this);
#if _CLIENT_
        //m_network_mng = new ClientNetManager();
#else
        m_network_mng = new ServerNetManager();
#endif
        if (null != m_network_mng)
        {
            m_network_mng.Init();
        }
        else
        {
            //Debug.LogError("fail to init network mng");
        }
        m_instance = this;

        SceneManager.LoadScene("Scenes/InGameScene");
        
    }

    // Update is called once per frame
    void Update()
    {
        //测试代码
        if (SceneManager.GetSceneByBuildIndex(1).isLoaded)
        {
            GetOpEn().Update();
        }
    }

    private void OnDestroy()
    {
        if (null != m_network_mng)
        {
            m_network_mng.Quit();
        }
    }
}
