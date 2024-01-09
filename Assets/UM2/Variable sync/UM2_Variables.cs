using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

public class LocalServerVariable{
    string value;
    Type type;
    public string name;
    UM2_Client client;

    public LocalServerVariable(string setName, object initialValue, Type setType, UM2_Client setClient, Action<string> callback = null){
        type = setType;
        name = setName;
        client = setClient;
        value = initialValue + "";

        try
        {
            this.getValue();
        }
        catch (System.Exception)
        {
            Debug.LogError("Could not parse initial value: " + initialValue + " into " + setType);
            return;
        }
        client.messageServer("newVar~" + name + "~" + value + "~" + type);
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

    public void setValue(string newValue){
        value = newValue;
        //Debug.Log(name + " = " + value);
    }
}

public class UM2_Variables : MonoBehaviour
{
    List<LocalServerVariable> serverVariables = new List<LocalServerVariable>();
    UM2_Client client;
	public static UM2_Variables instance;
    
    List<Type> allowedVariableTypes = new List<Type>{typeof(String), typeof(int), typeof(float)};

    public void syncVar(string name, string value){
        getServerVariable(name).setValue(value);
    }

    public void syncNewVar(string name, string value, string type){
        Type varType = Type.GetType(type);

        if(varType == typeof(int)){
            createServerVariable<int>(name, int.Parse(value));
        }
        else if(varType == typeof(float)){
            createServerVariable<float>(name, float.Parse(value));
        }
        else if(varType == typeof(string)){
            createServerVariable<string>(name, value);
        }
    }

	public void createServerVariable<T>(string name, T initialValue){
        foreach(LocalServerVariable serverVariable in serverVariables){
            if(serverVariable.name == name){
                Debug.LogError("A server variable with name " + name + " already exists");
                return;
            }
        }

        if(!allowedVariableTypes.Contains(typeof(T))){
            Debug.LogError("Type \"" + typeof(T) + "\" is not allowed to be a server variable.\nIt must be a string, int, or float (lists will be possible in the future)");
            return;
        }

		LocalServerVariable newVariable = new LocalServerVariable(name, initialValue, typeof(T), client);
		serverVariables.Add(newVariable);

        //Debug.Log("Created new server variable " + name);
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
        initialize();
    }

    public void addToVar(string name, object value){
        client.messageServer("addToVar~" + name + "~" + value);
    }

    public void setVar(string name, object value){
        client.messageServer("setVar~" + name + "~" + value);
    }

    async void initialize(){
        while (UM2_Client.clientID == -1){
            await Task.Delay(50);

            if(!UM2_Client.connectedToServer){
                return;
            }
        }
        
        createServerVariable<int>("testVar", 1);
        Invoke("testSet", 2);
        Invoke("testAdd", 3);
    }

    void testAdd(){
        addToVar("testVar", 1);
    }
    void testSet(){
        setVar("testVar", 2);
    }
}
