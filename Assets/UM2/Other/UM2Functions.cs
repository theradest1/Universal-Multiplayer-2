using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Reflection;
using System;

public class UM2Functions : MonoBehaviour
{
    public static void ConnectedToServer(){
        
    }

    List<UM2Functions> getChildScripts(){
        // Find all game objects in the scene
        GameObject[] gameObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();

        List<UM2Functions> childScripts = new List<UM2Functions>();

        // Iterate through each game object
        foreach (var gameObject in gameObjects)
        {
            // Add the matching scripts to the list
            childScripts.AddRange(gameObject.GetComponents<UM2Functions>());
        }

        return childScripts;
    }

    void callFunctionOfChildScripts(String functionName){
        List<UM2Functions> childScripts = getChildScripts();

        foreach(UM2Functions childScript in childScripts){
            childScript.Invoke(functionName);
        }
    }
}
