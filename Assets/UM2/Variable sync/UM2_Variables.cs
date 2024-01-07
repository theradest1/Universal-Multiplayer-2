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
    public int ID;

    public LocalServerVariable(string setName, object initialValue, Type setType, int setID){
        value = initialValue + "";
        type = setType;
        name = setName;
        ID = setID; 

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
    
    List<int> reservedIDs = new List<int>();
    public int targetReservedIDs = 3;
    
    List<Type> allowedVariableTypes = new List<Type>{typeof(String), typeof(int), typeof(float)};

    async void reserveIDLoop(){
        await Task.Delay(100);

        if(!UM2_Client.connectedToServer){
            return;
        }

        if(reservedIDs.Count < targetReservedIDs){
            getReserveNewObjectID();
        }

        reserveIDLoop();
    }

    void getReserveNewObjectID(){
        client.messageServer("reserveVariableID");
    }

    public void reservedVariableID(int newReservedID){
        reservedIDs.Add(newReservedID);
    }

	async public void createServerVariable<T>(string name, T initialValue){
        if(!allowedVariableTypes.Contains(typeof(T))){
            Debug.LogError("Type \"" + typeof(T) + "\" is not allowed to be a server variable.\nIt must be a string, int, or float (lists will be possible in the future)");
            return;
        }

        /*
        TODO:
        - send stuff to server
        */

        while(reservedIDs.Count == 0){
            getReserveNewObjectID();
            await Task.Delay(50);
        }

        int usedVariableID = reservedIDs[0];
        reservedIDs.RemoveAt(0);

		LocalServerVariable newVariable = new LocalServerVariable(name, initialValue, typeof(T), usedVariableID);
		serverVariables.Add(newVariable);

        Debug.Log("Created new server variable");
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
        reserveIDLoop();
    }
}
