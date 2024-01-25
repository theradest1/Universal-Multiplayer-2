using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonoBehaviourUM2 : MonoBehaviour
{
    void OnEnable()
    {
        //you can also put these lines in a regular monobehaviour script 
        //if u dont want to derive from this class
        //(dont forget the remove lines)
        //universal methods wont work
        UM2_Methods.addToGlobalMethods(this);
        UM2_Methods.addToServerMethods(this);
    }

    void OnDisable(){
        UM2_Methods.removeFromGlobalMethods(this);
        UM2_Methods.removeFromServerMethods(this);
    }
    

    //leave all of these blank (they are overriden)
    public virtual void OnConnect(int clientID){}
    //public virtual void OnDisconnect(){}
    public virtual void OnPlayerJoin(int clientID){}
    public virtual void OnPlayerLeave(int clientID){}
}
