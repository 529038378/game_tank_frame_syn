using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventPredefined 
{
    public enum MsgType
    {
        EMT_ENTITY_EVENT = 100,
        EMT_ENTITY_CHANGE_SCENE,
        EMT_ENTER_GAME,
    }

    public enum EntityEventType
    {
        ET_INVALID = -1,
        ET_CREATE = 0,
        ET_OP,
        ET_DESTROY,
        ET_COUNT,
    }

#if !_CLIENT_
    //局内的最多玩家数量
    public const int max_player = 1;
#endif 
}
