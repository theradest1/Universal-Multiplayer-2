using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;


public class UM2_Variables : MonoBehaviourUM2
{
    UM2_Client client;
	public static UM2_Variables instance;
    List<Type> allowedVariableTypes = new List<Type>{typeof(String), typeof(int), typeof(float)};
    List<NetworkVariable_Client> networkvariables = new List<NetworkVariable_Client>();

    public void syncVar(string name, string value){
        getNetworkVariable(name).setValue(value);
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
        foreach(NetworkVariable_Client networkVariable in networkvariables){
            if(networkVariable.name == name){
                Debug.LogError("A server variable with name " + name + " already exists");
                return;
            }
        }

        if(!allowedVariableTypes.Contains(typeof(T))){
            Debug.LogError("Type \"" + typeof(T) + "\" is not allowed to be a server variable.\nIt must be a string, int, or float (lists will be possible in the future)");
            return;
        }

		NetworkVariable_Client newVariable = new NetworkVariable_Client(name, initialValue, typeof(T), client);
		networkvariables.Add(newVariable);

		return;
	}

    public NetworkVariable_Client getNetworkVariable(string name){ //I need to make this a dictionary in the future
		foreach (NetworkVariable_Client networkVariable in networkvariables)
        {
            if(networkVariable.name == name){
                return networkVariable;
            }
        }
        
        Debug.LogError("Could not find server variable: " + name);
        return null;
    }

    public object getNetworkVariableValue(string name){
        return getNetworkVariable(name).getValue();
    }

	private void Awake()
	{
		instance = this;
	}

    void Start()
    {
        client = gameObject.GetComponent<UM2_Client>();
    }

    public override void OnConnect(int clientID){
        UM2_Methods.networkMethodServer("giveAllVariables", UM2_Client.clientID);
    }
}

public class NetworkVariable_Client
{
    public string name;
    object value;
    Type type;
    public int variableID;
    public bool serverVariable;

    public NetworkVariable_Client(string name, object value, Type type, bool serverVariable = false, bool newVariable = true){//, Action<string> callback = null){
        this.name = name;
        this.value = value;
        this.type = type;
        this.serverVariable = serverVariable;

        if(newVariable){
            UM2_Methods.networkMethodServer("newServerVar", name, value, type);
        }
    }

    public void sendValue(){
        UM2_Methods.networkMethodServer("setServerVar", name, value, type);
    }

    public object getValue(){
        if (type == typeof(int))
        {
            return (int)value;
        }
        else if (type == typeof(float))
        {
            return (float)value;
        }
        else if (type == typeof(string))
        {
            return (string)value;
        }
        Debug.LogError("Unknown server variable type: " + type);
        return null;
    }

    public void setValue(string newValue){
        value = newValue;
        sendValue();
    }
}