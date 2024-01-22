using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonoBehaviourUM2 : MonoBehaviour
{
    void OnEnable()
    {
        UM2_Methods.addToCallback(this);
    }

    void OnDisable(){
        UM2_Methods.removeFromCallback(this);
    }
    

    //leave all of these blank (they are overriden)
    public virtual void OnConnect(){}
    public virtual void OnDisconnect(){}
    public virtual void OnPlayerJoin(){}
    public virtual void OnPlayerLeave(){}
}
