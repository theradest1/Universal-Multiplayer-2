using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
using System.Linq;

public class UM2_Methods : MonoBehaviourUM2
{
    /*
    this script keeps track of all of the scripts 
    inhariting the methods of MonoBehaviourUM2
    so they can be called efficiently
    
    look in MonoBehaviourUM2 for the global methods
    */

    static List<MonoBehaviour> globalMethodScripts = new List<MonoBehaviour>();
    static List<MonoBehaviour> serverMethodScripts = new List<MonoBehaviour>();


    public static void addToGlobalMethods(MonoBehaviour scriptToAdd){
        globalMethodScripts.Add(scriptToAdd);
    }

    public static void addToServerMethods(MonoBehaviour scriptToAdd){
        serverMethodScripts.Add(scriptToAdd);
    }

    public static void removeFromGlobalMethods(MonoBehaviour scriptToRemove){
        if(globalMethodScripts.Contains(scriptToRemove)){
            globalMethodScripts.Remove(scriptToRemove);
        }
        else{
            Debug.LogWarning("Tried to remove class from subscription list that wasnt there");
        }
    }

    public static void removeFromServerMethods(MonoBehaviour scriptToRemove){
        if(serverMethodScripts.Contains(scriptToRemove)){
            serverMethodScripts.Remove(scriptToRemove);
        }
        else{
            Debug.LogWarning("Tried to remove class from subscription list that wasnt there");
        }
    }

    public static void callGlobalMethod(String methodName, object[] perameters = null){
        foreach(MonoBehaviour subscribedScript in globalMethodScripts){
            MethodInfo methodInfo = subscribedScript.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            methodInfo.Invoke(subscribedScript, perameters);
        }
    }

    public static void callServerMethod(string methodName, object[] perameters){
        foreach(MonoBehaviour subscribedScript in serverMethodScripts){
            MethodInfo methodInfo = subscribedScript.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if(methodInfo != null){
                methodInfo.Invoke(subscribedScript, perameters);
            }
        }
    }

    public static void networkMethodServer(string methodName, params object[] parameters){
        string message = methodName + "~" + String.Join("~", parameters);
        message = "server~" + message;
        UM2_Client.client.sendMessage(message, true, false);
    }
    public static void networkMethodOthers(string methodName, params object[] parameters){
        string message = methodName + "~" + String.Join("~", parameters);
        message = "others~" + message;
        UM2_Client.client.sendMessage(message, true, false);
    }
    public static void networkMethodGlobal(string methodName, params object[] parameters){
        string message = methodName + "~" + String.Join("~", parameters);
        message = "all~" + message;
        UM2_Client.client.sendMessage(message, true, false);
    }
    public static void networkMethodDirect(int recipientID, string methodName, params object[] parameters){
        string message = methodName + "~" + String.Join("~", parameters);
        message = "direct~" + recipientID + "~" + message;
        UM2_Client.client.sendMessage(message, true, false);
    }
}
