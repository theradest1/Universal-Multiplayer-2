using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.Data.Common;


public class UM2_Variables : MonoBehaviourUM2
{
    UM2_Client client;
	public static UM2_Variables instance;
    public List<Type> allowedVariableTypes = new List<Type>{typeof(String), typeof(int), typeof(float)};
    [HideInInspector] public List<NetworkVariable_Client> networkVariables = new List<NetworkVariable_Client>();
    public List<String> networkVariableNames = new List<String>();
    
    List<int> reservedVariableIDs = new List<int>();
    public int targetPreppedVariableIDs = 10;

    public override void OnConnect(int clientID)
    {
        UM2_Methods.networkMethodServer("giveAllVariables", UM2_Client.clientID);
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
        //Debug.Log("Reserved varible id " + id);
        reservedVariableIDs.Add(id);
    }

    public void syncVar(string name, string value){
        NetworkVariable_Client variable =  getNetworkVariable(name);
        if(variable == null){
            Debug.LogWarning("Could not find variable (requesting from server): " + name);
            UM2_Methods.networkMethodServer("giveAllVariables");
        }
        else{
            variable.setValue(value, false);
        }
    }

    public void syncNewVar(string name, int id, string value, Type type, int linkedID){
        //check if it already exists (only if it isnt an object based variable)
        if(linkedID == -1){
            foreach(NetworkVariable_Client networkVariable in networkVariables)
            {
                if(networkVariable.name == name){
                    Debug.Log("Variable " + name + " already exists, just setting value and ignoring creation");
                    networkVariable.setValue(value);
                    return;
                }
            }
        }

        new NetworkVariable_Client(name, value, type, id, linkedID);
    }
    
    public static void createNetworkVariable<T>(string name, T initialValue){
        UM2_Variables thisScript = UM2_Variables.instance;
        
        //make sure there arent name duplicates (remove this in the future)
        foreach(NetworkVariable_Client networkVariable in thisScript.networkVariables){
            if(networkVariable.name == name){
                Debug.LogError("A server variable with name " + name + " already exists");
                return;
            }
        }

        //make sure it is a valid type
        if(!thisScript.allowedVariableTypes.Contains(typeof(T))){
            Debug.LogError("Type \"" + typeof(T) + "\" is not allowed to be a server variable.\nIt must be a string, int, or float");
            return;
        }

        thisScript.StartCoroutine(thisScript.createNetworkVariable(name, initialValue, typeof(T)));

		return;
	}

    public IEnumerator createNetworkVariable(string name, object value, Type type){
        Debug.Log("Waiting for a reserved variable ID...");

        //wait until there are reserved IDs for variable before creating
        //its possible that this could create some issues if another variable is being created at the same time, but I don't think that will happen
        yield return new WaitUntil(() => reservedVariableIDs.Count > 0);

        int id = reservedVariableIDs[0];
        reservedVariableIDs.RemoveAt(0);

        NetworkVariable_Client newVariable = new NetworkVariable_Client(name, value, type, id);
        
        yield return null;
    }

    public static NetworkVariable_Client getNetworkVariable(string name, int linkedID = -1){ //I need to make this a dictionary in the future
		foreach (NetworkVariable_Client networkVariable in UM2_Variables.instance.networkVariables)
        {
            //if linkedID is -1, it isnt an object based network variable
            if(networkVariable.linkedID == linkedID && networkVariable.name == name){   
                return networkVariable;
            }
        }
        
        Debug.LogError("Could not find server variable: " + name);
        return null;
    }

    public static object getNetworkVariableValue(string name, int linkedID = -1){
        return getNetworkVariable(name, linkedID).getValue();
    }

    public static void setNetworkVariableValue(string name, object value, int linkedID = -1){
        getNetworkVariable(name, linkedID).setValue(value);
    }

    public static void addToNetworkVariableValue(string name, object valueToAdd, int linkedID = -1){
        getNetworkVariable(name, linkedID).addToValue(valueToAdd);
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

public class NetworkVariable_Client
{
    //basics
    public string name;
    object value;
    Type type;
    public int id;
    UM2_Variables variables;
    Action callback;

    public int linkedID; //for object based variable only, it will stay -1 if it isn't an object based variable

    public NetworkVariable_Client(string name, object value, Type type, int id, int linkedID = -1, Action callbackOnChange = null){//, Action<string> callback = null){
        Debug.Log("Created network variable: " + name);

        this.name = name;
        this.id = id;
        this.value = value;
        this.type = type;
        this.linkedID = linkedID;

        this.callback = callbackOnChange;

        UM2_Variables.instance.networkVariableNames.Add(name);
        UM2_Variables.instance.networkVariables.Add(this);
        UM2_Methods.networkMethodServer("newVar", name, id, value, type, linkedID);
        Debug.Log("yuh");
    }

    public void sendValue(){
        UM2_Methods.networkMethodServer("setVarValue", id, value, linkedID);
    }

    public void addToValue(object valueToAdd){
        UM2_Methods.networkMethodServer("addToVarValue", id, valueToAdd, linkedID);
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

    public void setValue(object newValue, bool sync = true){
        value = newValue;

        if(callback != null){
            callback.Invoke();
        }

        if(sync){
            sendValue();
        }
    }
}

public class ObjectNetworkVariableAttribute : PropertyAttribute
{
    //nothing here, it is just a way to seprate normal and network variables
}