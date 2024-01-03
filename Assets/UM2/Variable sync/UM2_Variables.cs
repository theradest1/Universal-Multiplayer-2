using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class LocalServerVariable{
    string value;
    Type type;
    public string name;
    List<int> reservedIDs = new List<int>();

    public LocalServerVariable(string setName, object initialValue, Type setType){
        value = initialValue + "";
        type = setType;
        name = setName;

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

    public object getValue(){
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

    public void setValue(object newValue){
        value = newValue + "";
        syncVariable();
    }

    public void syncVariable(){
        Debug.LogWarning("Not implemented");
    }
}

public class UM2_Variables : MonoBehaviour
{
    List<LocalServerVariable> serverVariables = new List<LocalServerVariable>();
    public UM2_Client client;
	public static UM2_Variables instance;

	//I could make it so you can pass any type, but you cant actually do that (it will only be able to sync basic variables)
	public LocalServerVariable createServerVariable(string name, string initialValue){
		LocalServerVariable newVariable = new LocalServerVariable(name, initialValue, initialValue.GetType());
		serverVariables.Add(newVariable);
		return newVariable;
	}
    public LocalServerVariable createServerVariable(string name, int initialValue){
		LocalServerVariable newVariable = new LocalServerVariable(name, initialValue, initialValue.GetType());
		serverVariables.Add(newVariable);
		return newVariable;
	}
    public LocalServerVariable createServerVariable(string name, float initialValue){
        LocalServerVariable newVariable = new LocalServerVariable(name, initialValue, initialValue.GetType());
		serverVariables.Add(newVariable);
        return newVariable;
    }

    public LocalServerVariable getServerVariable(string name){ //I need to make this a dictionary in the future
		foreach (LocalServerVariable variable in serverVariables)
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

	private void Awake()
	{
		instance = this;
	}
}
