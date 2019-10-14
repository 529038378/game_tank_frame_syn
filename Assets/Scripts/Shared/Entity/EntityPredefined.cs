using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityPredefined 
{
    //玩家阵营
    public enum EntityCampType
    {
        ECT_PLAYER = 0,
        ECT_ENEMY,
        ECT_COUNT,
    };

    //实体类型
    public enum EntityType
    {
        EET_INVALID = -1,
        EET_TANK = 0,
        EET_BULLET,

        EET_COUNT,
    }

    //实体操作状态
    public enum EntityOpType
    {
        EOT_INVALID = -1,
        EOT_IDLE  = 0,
        EOT_FORWARD,
        EOT_BACKWARD,
        EOT_LEFT,
        EOT_RIGHT,
        EOT_FIRE,
    };

    public const string SceneCamera = "InGameFollowCamera";

    public const float tank_speed = 1.0f;
    public static Vector3 spwan_pos0 = new Vector3(0, 0, 0);
    public static Vector3 spwan_pos1 = new Vector3(10, 0, 0);
}
