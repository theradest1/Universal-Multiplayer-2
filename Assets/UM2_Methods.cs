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
        //get all enabled game object in the scene
        GameObject[] allGameObjects = GameObject.FindObjectsOfType<GameObject>();

        foreach (GameObject gameObject in allGameObjects) {

            //get all monobehaviours on the game object
            MonoBehaviour[] scripts = gameObject.GetComponents<MonoBehaviour>();
            foreach(MonoBehaviour script in scripts){

                //get method info (makes it possible to call private methods)
                MethodInfo methodInfo = script.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

                //if the method exists in the Monobehaviour
                if(methodInfo != null){
                    //try-catch for if parameters don't match up
                    try
                    {
                        //run the method with optional parameters
                        methodInfo.Invoke(script, perameters);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError("Error when calling global method: " + e);
                    }
                }
            }
        }
    }

    void Start()
    {
        callGlobalMethod("ConnectedToServer");
    }
}
