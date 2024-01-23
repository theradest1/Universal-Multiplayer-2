using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;

public class UM2_Methods : MonoBehaviourUM2
{
    /*
    this script keeps track of all of the scripts 
    inhariting the methods of MonoBehaviourUM2
    so they can be called efficiently
    
    look in MonoBehaviourUM2 for the methods
    */

    public static List<MonoBehaviourUM2> subscribedScripts = new List<MonoBehaviourUM2>();


    public static void addToCallback(MonoBehaviourUM2 classToAdd){
        subscribedScripts.Add(classToAdd);
    }

    public static void removeFromCallback(MonoBehaviourUM2 classToRemove){
        if(subscribedScripts.Contains(classToRemove)){
            subscribedScripts.Remove(classToRemove);
        }
        else{
            Debug.LogWarning("Tried to remove class from subscription list that wasnt there");
        }
    }

    public static void callGlobalMethod(String methodName, object[] perameters = null){
        foreach(MonoBehaviourUM2 subscribedClass in subscribedScripts){
            MethodInfo methodInfo = subscribedClass.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            methodInfo.Invoke(subscribedClass, perameters);
        }
    }
}
