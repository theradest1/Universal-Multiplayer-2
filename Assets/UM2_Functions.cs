using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class UM2_Functions : MonoBehaviour
{
    GameObject[] allGameObjects;
    void Start()
    {
        refreshGameObjectList();
    }

    void refreshGameObjectList(){
        //this is sketch as fuck and I really shouldnt be doing it
        //but I dont really care
        allGameObjects = GameObject.FindObjectsOfType<GameObject>();
    }

    public void callGlobalMethod(String methodName){
        refreshGameObjectList();
        
        foreach (GameObject gameObject in allGameObjects) {
            gameObject.SendMessage(methodName, SendMessageOptions.DontRequireReceiver);
        }
    }
}
