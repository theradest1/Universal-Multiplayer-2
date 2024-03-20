using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using Unity.VisualScripting;
using System.Data.Common;


public class UM2_Variables : MonoBehaviourUM2
{
    UM2_Client client;
	public static UM2_Variables instance;
    List<Type> allowedVariableTypes = new List<Type>{typeof(String), typeof(int), typeof(float)};
    List<NetworkVariable_Client> networkvariables = new List<NetworkVariable_Client>();
    public List<String> networkVariableNames = new List<String>();
    
    List<int> reservedVariableIDs = new List<int>();
    public int targetPreppedVariableIDs = 10;

    public virtual void OnConnectedToServer() {
        reserveIDLoop();
    }
    
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
        //make sure there arent name duplicates (remove this in the future)
        foreach(NetworkVariable_Client networkVariable in networkvariables){
            if(networkVariable.name == name){
                Debug.LogError("A server variable with name " + name + " already exists");
                return;
            }
        }

        //make sure it is a valid type
        if(!allowedVariableTypes.Contains(typeof(T))){
            Debug.LogError("Type \"" + typeof(T) + "\" is not allowed to be a server variable.\nIt must be a string, int, or float");
            return;
        }

        StartCoroutine(createNetworkVariable(name, initialValue, typeof(T)));

		return;
	}

    IEnumerator createNetworkVariable(string name, object value, Type type){
        Debug.Log("Waiting for a reserved variable ID...");

        //wait until there are reserved IDs for variable before creating
        yield return new WaitUntil(() => reservedVariableIDs.Count > 0);

        int id = reservedVariableIDs[0];
        reservedVariableIDs.RemoveAt(0);

        NetworkVariable_Client newVariable = new NetworkVariable_Client(name, value, type, id);
        
        yield return null;
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

    public NetworkVariable_Client(string name, object value, Type type, int id){//, Action<string> callback = null){
        Debug.Log("Created network variable: " + name);

        this.name = name;
        this.value = value;
        this.type = type;
        this.id = id;

        UM2_Variables.instance.networkVariableNames.Add(name);
        UM2_Methods.networkMethodServer("newVar", name, id, value, type);
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