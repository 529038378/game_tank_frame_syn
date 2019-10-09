using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankEntity : CEntity
{
    public TankEntity()
    {
        Init();
    }
    string private_path = "Prefabs/Tank";
    public override void Init()
    {
        if (null == m_en_obj)
        {
            Object obj = Resources.Load(private_path);
            m_en_obj = GameObject.Instantiate(obj) as GameObject;
            if (null == m_en_obj)
            {
                Debug.Log("Fail to create tank");
            }
        }
        m_op_type = EntityPredefined.EntityOpType.EOT_IDLE;
    }
    //逻辑层相关的东西
    Vector3 target_pos;
    Vector3 cur_pos;

    protected EntityPredefined.EntityOpType m_op_type;
    protected EntityPredefined.EntityCampType m_camp_type;
    float acc_time;
    const float fixed_time = 0.02f;
    const float speed = 1.0f;
    void UpdatePosLerp()
    {
        acc_time += Time.deltaTime;
        while(acc_time > fixed_time)
        {
            m_en_obj.transform.position = target_pos;
            RecordCurPos();
            UpdateTargetPos(m_op_type);
            acc_time -= fixed_time;
        }
        float ratio = (acc_time % fixed_time) / fixed_time;
        m_en_obj.transform.position = Vector3.Lerp(cur_pos, target_pos, acc_time);
    }

    void Fire()
    {

    }
    public override GameObject GetObj()
    {
        return m_en_obj;
    }
    public override void Update()
    {
        if (null == GetObj())
        {
            return;
        }

        base.Update();
        
        switch(m_op_type)
        {
            case EntityPredefined.EntityOpType.EOT_IDLE:
            break;
            case EntityPredefined.EntityOpType.EOT_FORWARD:
            case EntityPredefined.EntityOpType.EOT_BACKWARD:
            case EntityPredefined.EntityOpType.EOT_LEFT:
            case EntityPredefined.EntityOpType.EOT_RIGHT:
            UpdatePosLerp();
            break;
            case EntityPredefined.EntityOpType.EOT_FIRE:
            Fire();
            break;
        }

    }

    public void UpdateTargetPos(EntityPredefined.EntityOpType op_type)
    {
        target_pos += m_en_obj.transform.forward * speed * fixed_time;
    }

    void UpdateRotation(EntityPredefined.EntityOpType op_type)
    {
        if (op_type != m_op_type)
        {
            Vector3 axis = new Vector3(0, 1, 0);
            float angle = 0.0f;
            switch(op_type)
            {
                case EntityPredefined.EntityOpType.EOT_BACKWARD:
                angle = 180;
                break;
                case EntityPredefined.EntityOpType.EOT_LEFT:
                angle = -90;
                break;
                case EntityPredefined.EntityOpType.EOT_RIGHT:
                angle = 90;
                break;
            }
            m_en_obj.transform.RotateAroundLocal(axis, Mathf.Deg2Rad * angle);
        }
    }

    public override void Op(EntityPredefined.EntityOpType op_type)
    {
        base.Op(op_type);
        
        int offset = (int) (op_type - EntityPredefined.EntityOpType.EOT_FORWARD);
        if (offset <= (int)EntityPredefined.EntityOpType.EOT_RIGHT)
        {
            RecordCurPos();
            UpdateRotation(op_type);
            UpdateTargetPos(op_type);
        }
                
        //跟前面的api调用顺序相关
        m_op_type = op_type;
    }
    public override void RecordCurPos()
    {
        cur_pos = target_pos;
    }
}
