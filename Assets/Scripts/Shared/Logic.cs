using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

public class Logic : MonoBehaviour
{
    private void Start()
    {
    }
#if !_CLIENT_
    public void SwitchServerFrameSyn()
    {
        FrameSynLogic.StopFrameSyn = !FrameSynLogic.StopFrameSyn;
    }
#endif

#if _CLIENT_
    IEntity m_op_en = null;
    public IEntity GetOpEn()
    {
        return m_op_en;
    }
    public void SetOpEn(IEntity en)
    {
        m_op_en = en;
    }
#endif
    static Logic m_instance = null;
    public static Logic Instance()
    {
        return m_instance;
    }
    public INetManager GetNetMng()
    {
        return m_network_mng;
    }
    public IReplayMng GetReplayMng()
    {
        return m_replay_mng;
    }
    // Start is called before the first frame update
    static INetManager m_network_mng = null;
    ISceneMng m_scene_mng = null;
    IReplayMng m_replay_mng = null;
    bool m_has_start = false;
    void Awake()
    {
        if(m_has_start)
        {
            return;
        }
        m_has_start = true;
        m_scene_mng = new CSceneMng();
        DontDestroyOnLoad(this);
        INetManagerCallback net_callback = m_scene_mng as CSceneMng;
#if _CLIENT_
        m_network_mng = new ClientNetManager(net_callback);
#else
        m_network_mng = new ServerNetManager(net_callback);
#endif
        if (null != m_network_mng)
        {
            m_network_mng.Init();
        }
        else
        {
            Debug.LogError("fail to init network mng");
        }
        m_instance = this;
        m_replay_mng = new CReplayMng();

        m_entry_cam = GameObject.Find("Main Camera");
        m_ingame_cam = GameObject.Find("InGameFollowCamera");
        SceneManager.sceneUnloaded += OnInGameSceneUnload;
    }
    GameObject m_entry_cam;
    GameObject m_ingame_cam;
    public IFrameSyn FrameSynLogic
    {
        get
        {
            if (null == GetSceneMng())
            {
                return null;
            }
            return GetSceneMng().FrameSynLogic;
        }
    }
#if _CLIENT_
    public void RequestEnterGame()
    {
        m_network_mng.Send((short)EventPredefined.MsgType.EMT_ENTER_GAME, new CEnterInGameEvent());
    }

    public void NotifyClientReady()
    {
        m_network_mng.Send((short)EventPredefined.MsgType.EMT_CLIENT_READY, new CClientReadyEvent());
    }
    public void NotifyClientQuit()
    {
        m_network_mng.Send((short) EventPredefined.MsgType.EMT_CLIENT_LEAVE_INGAME, new CClientLeaveInGameEvent());
    }
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
#endif
    Scene m_in_game_scene;
    Scene m_entry_scene;
    void OnInGameSceneUnload(Scene cur_scene)
    {
        
    }
    void ChangeToInGameScene()
    {
        if(!m_in_game_scene.isLoaded)
        {
            m_in_game_scene = SceneManager.LoadScene("Scenes/InGameScene", new LoadSceneParameters(LoadSceneMode.Additive));
        }
        else
        {
            SceneManager.SetActiveScene(m_in_game_scene);
        }
        if(m_entry_cam)
        {
            m_entry_cam.SetActive(false);
        }
    }
    void ChangeToEntryScene()
    {
        if (m_in_game_scene.isLoaded)
        {
            if (!SceneManager.UnloadScene(m_in_game_scene))
            {
                Debug.Log(" fail to unload scene ");
            }
        }
        //相机切换
        if (m_entry_cam)
        {
            m_entry_cam.SetActive(true);
        }
    }
    public void EnterInGame()
    {
#if _CLIENT_
        ChangeToInGameScene();
#endif
        m_scene_mng.Enter();
    }
    public void LeaveGame()
    {
        m_scene_mng.Leave();
#if _CLIENT_
        NotifyClientQuit();
        ChangeToEntryScene();
#endif
    }
    // Update is called once per frame
    void Update()
    {
#if !_CLIENT_
        if (null != m_network_mng)
        {
            m_network_mng.Update();
        }
#endif
       
        if (null != m_scene_mng && m_scene_mng.InScene && !m_replay_mng.IsInReplay)
        {
            m_scene_mng.Update();
        }
        if (null != m_replay_mng && m_replay_mng.IsInReplay)
        {
            m_replay_mng.Update();
        }

        //Debug.Log(" cur loaded scene count : " + SceneManager.sceneCount);
    }

    private void OnDestroy()
    {
        if (null != m_network_mng)
        {
            m_network_mng.Leave();
        }
        if (null != m_scene_mng)
        {
            m_scene_mng.Leave();
        }
    }

    public ISceneMng GetSceneMng()
    {
        return m_scene_mng;
    }
#if _CLIENT_
    public IColliderCallback GetCollCallback()
    {
        IColliderCallback callback = m_scene_mng as IColliderCallback;
        return callback;
    }
#endif
}
