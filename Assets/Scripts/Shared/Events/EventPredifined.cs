using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventPredefined 
{
    public enum MsgType
    {
        EMT_ENTITY_CHANGE_SCENE = 100, 
        EMT_CLIENT_READY,
        EMT_ENTER_GAME,
        EMT_QUIT_GAME,
        EMT_ENTITY_CREATE,
        EMT_ENTITY_DESTROY,
        EMT_ENTITY_OP,
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
