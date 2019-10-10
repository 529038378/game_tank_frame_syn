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
        EOT_IDLE = -1,
        EOT_FORWARD = 0,
        EOT_BACKWARD,
        EOT_LEFT,
        EOT_RIGHT,
        EOT_FIRE,
    };

    public const string SceneCamera = "Camera";
}
