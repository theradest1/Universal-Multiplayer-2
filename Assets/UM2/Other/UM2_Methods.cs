using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;

public class UM2_Methods : MonoBehaviour
{

    public static List<MonoBehaviourUM2> subscribedClasses = new List<MonoBehaviourUM2>();

    //look in MonoBehaviourUM2 for used methods

    public static void addToCallback(MonoBehaviourUM2 classToAdd){
        subscribedClasses.Add(classToAdd);
    }

    public static void removeFromCallback(MonoBehaviourUM2 classToRemove){
        if(subscribedClasses.Contains(classToRemove)){
            subscribedClasses.Remove(classToRemove);
        }
        else{
            Debug.LogWarning("Tried to remove class from subscription list that wasnt there");
        }
    }

    public static void callGlobalMethod(String methodName, object[] perameters = null){
        foreach(MonoBehaviourUM2 subscribedClass in subscribedClasses){
            MethodInfo methodInfo = subscribedClass.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            methodInfo.Invoke(subscribedClass, perameters);
        }
    }
}
