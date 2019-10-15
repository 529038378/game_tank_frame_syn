using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if _CLIENT_
public interface IColliderCallback 
{
    void OnCollision(Collider en);
}
#endif
