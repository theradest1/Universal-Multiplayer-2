using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestClass : MonoBehaviour
{
    void OnEnable()
    {
        //add to subscription list
        Debug.Log("Yuh");
    }

    void OnDisable(){
        //remove from subscription list
    }

    virtual public void OnConnected(){
    
    }



}
