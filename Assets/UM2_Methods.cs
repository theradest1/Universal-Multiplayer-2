using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;

public class UM2_Methods : MonoBehaviour
{
    /*
    Global functions used by UM2:
        Implemented:

        Working on:    
            Connected()
            Disconnected()
            PlayerJoined()
            PlayerLeft()
    */

    public void callGlobalMethod(String methodName, object[] perameters = null){
        //I know this is kind of a terrible implementation
        //that breaks some pretty set-in-stone-rules of programming
        //but I dont really care - its all for ease of access

        //get all current enabled game object in the scene
        GameObject[] allGameObjects = GameObject.FindObjectsOfType<GameObject>();
        foreach (GameObject gameObject in allGameObjects) {
            //get all monobehaviours on the game object
            MonoBehaviour[] scripts = gameObject.GetComponents<MonoBehaviour>();
            foreach(MonoBehaviour script in scripts){
                //get method info (makes it possible to call private functions)
                MethodInfo methodInfo = script.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                if(methodInfo != null){
                    //run the method
                    try
                    {
                        methodInfo.Invoke(script, perameters);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError("Error when calling global UM2 function: " + e);
                    }
                }
            }
            //gameObject.SendMessage(methodName, SendMessageOptions.DontRequireReceiver);
        }
    }

    void Start()
    {
        callGlobalMethod("ConnectedToServer");
    }
}
