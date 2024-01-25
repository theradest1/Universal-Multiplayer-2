using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
using System.Linq;
using UnityEngine.UIElements;
using Unity.VisualScripting;

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

    public static void invokeNetworkMethod(string methodName, string[] perameters){
        int perameterCount = perameters.Length;
        
        List<(MethodInfo, MonoBehaviour)> possibleMethodsAndScripts = new List<(MethodInfo, MonoBehaviour)>();
        
        foreach(MonoBehaviour subscribedScript in serverMethodScripts){
            //Debug.Log("subscribed: " + subscribedScript);
            MethodInfo methodInfo = subscribedScript.GetType().GetMethod(methodName);
            if(methodInfo != null && methodInfo.GetParameters().Length == perameterCount){
                possibleMethodsAndScripts.Add((methodInfo, subscribedScript));
            }
        }
        
        int succeededCalls = 0;
        int failedCalls = 0;
        foreach((MethodInfo methodInfo, MonoBehaviour script) in possibleMethodsAndScripts){
            try{
                //trying to parse perameters
                ParameterInfo[] methodPerameters = methodInfo.GetParameters();
                object[] parsedParameters = new object[methodPerameters.Length];
                for (int i = 0; i < methodPerameters.Length; i++)
                {
                    Type parameterType = methodPerameters[i].ParameterType;
                    object parsedValue = QuickMethods.ParseValue(perameters[i], parameterType);
                    parsedParameters[i] = parsedValue;
                }

                //try to call that method with parsed perameters
                methodInfo.Invoke(script, parsedParameters);
                succeededCalls++;
            }
            catch(Exception e){
                failedCalls++;
                Debug.LogError(e);
            }
        }

        //now thats a chunky debugging message tree
        string perameterString = QuickMethods.ArrayToString(perameters);
        if(succeededCalls == 0){
            if(failedCalls == 0){
                Debug.LogError("Function was not found: Name: " + methodName + " Perameters: " + perameterString + "\n1. make sure the parent script is a MonoBehaviourUM2 script\n2. Make sure the name is correct\n3. The perameter count must be the exact same as how it was called\n4. The method being called must be pubic\nTurn on message debugging on client script for some debugging help (:");
            }
            else{
                Debug.LogError("Function was found, and has same perameter count, but there was an error calling it. Name: " + methodName + " Perameters: " + perameterString + "\nIt is most likley the perameter types.");
            }
        }
        else if(failedCalls != 0){
            Debug.LogError(failedCalls + " network method(s) werent called because of an error. Name: " + methodName + " Perameters: " + perameterString + " (" + succeededCalls + " were called without a problem)");
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
