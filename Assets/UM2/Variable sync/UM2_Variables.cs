using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerVariable{
    string value;
    Type type;
    public string name;

    public ServerVariable(string name, object initialValue, Type setType){
        value = initialValue + "";
        type = setType;
        try
        {
            this.getValue();
        }
        catch (System.Exception)
        {
            Debug.LogError("Could not parse initial value: " + initialValue + "\nType: " + setType);
            return;
        }
    }

    public object parser(object value){
        if (type == typeof(int))
        {
            return int.Parse(value);
        }
        else if (type == typeof(float))
        {
            return float.Parse(value);
        }
        else if (type == typeof(string))
        {
            return value;
        }

        Debug.LogError("Unknown server variable type: " + type);
        return null;
    }

    public void setValue(object value){

    }
}

public class UM2_Variables : MonoBehaviour
{
    List<ServerVariable> serverVariables = new List<ServerVariable>();
    public UM2_Client client;

    //I could make it so you can pass any type, but you cant actually do that (it will only be able to sync basic variables)
    public ServerVariable createServerVariable(string name, string initialValue){
        return new ServerVariable(name, initialValue, initialValue.GetType());
    }
    public ServerVariable createServerVariable(string name, int initialValue){
        return new ServerVariable(name, initialValue, initialValue.GetType());
    }
    public ServerVariable createServerVariable(string name, float initialValue){
        return new ServerVariable(name, initialValue, initialValue.GetType());
    }

    public ServerVariable getServerVariable(string name){
        foreach (ServerVariable variable in serverVariables)
        {
            if(variable.name == name){
                return variable;
            }
        }
        
        Debug.LogError("Could not find server variable: " + name);
        return null;
    }

    public object getServerVariableValue(string name){
        return getServerVariable(name).getValue();
    }
}
