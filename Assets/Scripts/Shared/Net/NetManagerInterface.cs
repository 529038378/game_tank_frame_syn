using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public interface NetManagerInterface
{
    void Init();
    void Quit();
    void Send(short msg_type, MessageBase msg);
    void HandleMsg(MessageBase msg);
}
