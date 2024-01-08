using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Threading.Tasks;

public class LocalServerVariable{
    string value;
    Type type;
    public string name;

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
    UM2_Client client;
	public static UM2_Variables instance;
    
    List<Type> allowedVariableTypes = new List<Type>{typeof(String), typeof(int), typeof(float)};


	public void createServerVariable<T>(string name, T initialValue){
        if(!allowedVariableTypes.Contains(typeof(T))){
            Debug.LogError("Type \"" + typeof(T) + "\" is not allowed to be a server variable.\nIt must be a string, int, or float (lists will be possible in the future)");
            return;
        }

        /*
        TODO:
        - send stuff to server
        */

		LocalServerVariable newVariable = new LocalServerVariable(name, initialValue, typeof(T));
		serverVariables.Add(newVariable);

        Debug.Log("Created new server variable " + name);
		return;
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

    void Start()
    {
        client = gameObject.GetComponent<UM2_Client>();
    }
}
