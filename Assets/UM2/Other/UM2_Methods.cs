using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
using System.Linq;

public enum UM2_RecipientGroups{
    Server = -1,
    Global = -2,
    Others = -3
}

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

    public static void invokeNetworkMethod(UM2_RecipientGroups recipients, string methodName, params object[] parameters)
    {
        invokeNetworkMethod((int)recipients, methodName, parameters);
    }

    public static void invokeNetworkMethod(int recipient, string methodName, params object[] parameters)
    {
        string message = methodName + "~" + String.Join("~", parameters);
        switch(recipient){
            case (int)UM2_RecipientGroups.Global:
                Debug.Log("Global message");
                message = "all~" + message;
                UM2_Client.client.sendMessage(message, true, false);
                break;
            case (int)UM2_RecipientGroups.Others:
                Debug.Log("Others message");
                message = "others~" + message;
                UM2_Client.client.sendMessage(message, true, false);
                break;
            case (int)UM2_RecipientGroups.Server:
                message = "server~" + message;
                UM2_Client.client.sendMessage(message, true, false);
                break;
            default:
                message = "direct~" + recipient + "~" + message;
                UM2_Client.client.sendMessage(message, true, false);
                break;
        }
    }
}
