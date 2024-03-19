using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using Unity.VisualScripting;


public class UM2_Variables : MonoBehaviourUM2
{
    UM2_Client client;
	public static UM2_Variables instance;
    List<Type> allowedVariableTypes = new List<Type>{typeof(String), typeof(int), typeof(float)};
    List<NetworkVariable_Client> networkvariables = new List<NetworkVariable_Client>();
    public List<String> networkVariableNames = new List<String>();
    
    List<int> reservedVariableIDs = new List<int>();
    public int targetPreppedVariableIDs = 10;
    
    async void reserveIDLoop(){
        await Task.Delay(100);

        if(!UM2_Client.connectedToServer){
            return;
        }

        if(reservedVariableIDs.Count < targetPreppedVariableIDs){
            UM2_Methods.networkMethodServer("reserveVariableID");
        }

        reserveIDLoop();
    }

    public void reservedVariableID(int id){
        Debug.Log("Reserved varible id " + id);
        reservedVariableIDs.Add(id);
    }

    public void syncVar(string name, string value){
        getNetworkVariable(name).setValue(value);
    }

    public void syncNewVar(string name, int id, string value, Type type){
        //check if it already exists
        if(networkVariableNames.Contains(name)){
            Debug.Log("Variable " + name + " already exists");
            return;
        }

        NetworkVariable_Client newVariable = new NetworkVariable_Client(name, value, type, id);
        networkvariables.Add(newVariable);
    }
    
    public void createNetworkVariable<T>(string name, T initialValue){
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

		NetworkVariable_Client newVariable = new NetworkVariable_Client(name, initialValue, typeof(T));
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
    public int id;
    public bool serverVariable;
    UM2_Variables variables;

    public NetworkVariable_Client(string name, object value, Type type, int id = -1){//, Action<string> callback = null){
        this.name = name;
        this.value = value;
        this.type = type;

        StartCoroutine(initialize(id));
    }

    IEnumerator initialize(int id){
        UM2_Variables.instance.networkVariableNames.Add(name);
        
        if(id == -1){
            yield return new WaitUntil(() => UM2_Client.clientID != -1);

            UM2_Methods.networkMethodServer("newVar", name, id, value, type);
        }

        this.id = id;
    }

    public void sendValue(){
        UM2_Methods.networkMethodServer("setVarValue", name, value, type);
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