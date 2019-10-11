using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public abstract class IEvent : MessageBase
{
   public abstract EventPredefined.MsgType GetEventType();
}
