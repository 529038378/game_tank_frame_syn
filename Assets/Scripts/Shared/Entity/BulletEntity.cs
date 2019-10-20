using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if _CLIENT_
public class BulletEntity : CEntity
{
    public BulletEntity(Vector3 origin_pos, Vector3 forward_dir)
    {
        Init();

        if (null == m_en_obj)
        {
            return;
        }
        m_en_obj.transform.position = origin_pos;
        m_en_obj.transform.forward = forward_dir;
    }

    public override void Init()
    {
        if (null == m_en_obj)
        {
            Object obj = Resources.Load(EntityPredefined.BulletPrefabPath);
            m_en_obj = GameObject.Instantiate(obj) as GameObject;
            if (null == m_en_obj)
            {
                Debug.Log("Fail to create tank");
            }
        }
    }

    float acc_time = 0;
    public override void Update(int delta_time)
    {
        if (null == m_en_obj)
        {
            return;
        }

        base.Update(delta_time);

        if (CheckHit())
        {
            Logic.Instance().GetSceneMng().RecycleEn(this);
            return;
        }

        m_en_obj.transform.position += EntityPredefined.bullet_speed * delta_time / 1000 * m_en_obj.transform.forward;

        if (m_en_obj.transform.position.y < 0)
        {
            Logic.Instance().GetSceneMng().RecycleEn(this);
        }
    }

    //碰撞检测
    bool CheckHit()
    {
        bool hit = false;
        RaycastHit hit_info = new RaycastHit();
        Ray forward_ray = new Ray(m_en_obj.transform.position, m_en_obj.transform.forward);
        if (Physics.Raycast(forward_ray, out hit_info))
        {
            if(EntityPredefined.PlayerEnTag == hit_info.transform.tag )
            {
                float frame_instance = ( EntityPredefined.bullet_speed * EntityPredefined.render_update_gap * m_en_obj.transform.forward ).magnitude;
                float hit_distance = hit_info.distance;//Vector3.Distance(m_en_obj.transform.position, hit_info.transform.position);
                hit = hit_distance <= frame_instance;
                if (hit)
                {
                    Debug.Log("hit player! " +
                    " FrameIndex : " + Logic.Instance().FrameSynLogic.FrameIndex.ToString() +
                    " HitPos : " + hit_info.point.ToString());
                    Logic.Instance().GetCollCallback().OnCollision(hit_info.collider);
                }
            }
        }
        return hit;
    }
    public override void Destroy()
    {
        base.Destroy();
        DestoryImm();
    }
    public override void DestoryImm()
    {
        base.DestoryImm();
    }
}
#endif