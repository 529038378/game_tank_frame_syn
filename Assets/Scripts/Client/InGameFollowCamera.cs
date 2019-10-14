using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if _CLIENT_
public class InGameFollowCamera : MonoBehaviour
{
    //相机调整参数
    public int offset_radius = 5;
    public float head_point = 2.5f;
    GameObject m_follow_obj = null;
    Camera m_cam = null;
    public float h_degree = 0;
    public float v_degree = 0;
    void AdjustCamPos(bool init = false)
    {
        if(null != m_cam && null != m_follow_obj)
        {
            //计算方向
            if (init)
            {
                h_degree += m_follow_obj.transform.localEulerAngles.z;
            }
            Vector3 dir = new Vector3(Mathf.Cos(Mathf.Deg2Rad * v_degree ) * Mathf.Sin(Mathf.Deg2Rad * h_degree), Mathf.Sin(Mathf.Deg2Rad * v_degree), Mathf.Cos(Mathf.Deg2Rad * v_degree) * Mathf.Cos(Mathf.Deg2Rad * h_degree));
            m_cam.transform.position = m_follow_obj.transform.position + new Vector3(0, head_point, 0) + offset_radius * dir;
            m_cam.transform.LookAt(m_follow_obj.transform.position + new Vector3(0, head_point, 0));
            //Debug.Log("cam pos : " + m_cam.transform.position.ToString());   
        }
    }
    public void UpdateByFollow()
    {
        if (null == m_follow_obj && null != Logic.Instance().GetOpEn())
        {
            m_follow_obj = Logic.Instance().GetOpEn().GetObj();
        }
        m_cam = GetComponent<Camera>();
        AdjustCamPos(true);
    }
    
    void LateUpdate()
    {
       UpdateByFollow();
    }
}
#endif