using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.Data.Common;
using JetBrains.Annotations;


public class UM2_Variables : MonoBehaviourUM2
{
	public static UM2_Variables instance;
    UM2_Client client;


    public List<Type> allowedVariableTypes = new List<Type>{typeof(String), typeof(int), typeof(float)};
    [HideInInspector] public List<NetworkVariable_Client> networkVariables = new List<NetworkVariable_Client>();
    

    List<int> reservedVariableIDs = new List<int>();
    public int targetPreppedVariableIDs = 10;
    [SerializeField] int reserveIDDelayMS = 100;


    public override void OnConnect(int clientID)
    {
        UM2_Methods.networkMethodServer("giveAllVariables", UM2_Client.clientID);
        reserveIDLoop();
    }

    async void reserveIDLoop(){
        await Task.Delay(reserveIDDelayMS);

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

    public void syncVar(string name, string value, int linkedID){
        NetworkVariable_Client variable =  getNetworkVariable(name, linkedID);
        if(variable == null){
            Debug.LogWarning("Could not find variable (requesting from server): " + name);
            UM2_Methods.networkMethodServer("giveAllVariables");
        }
        else{
            variable.setValue(value, false);
        }
    }

    public void syncNewVar(string name, int id, string value, Type type, int linkedID){
        //check if it already exists, only set value if it already does
        foreach(NetworkVariable_Client networkVariable in networkVariables)
        {
            if(networkVariable.id == id){
                //Debug.Log("Variable with id " + id + " already exists, just setting value and ignoring creation");
                networkVariable.setValue(value);
                return;
            }
        }

        new NetworkVariable_Client(name, value, type, id, linkedID);
    }
    
    public static void createNetworkVariable<T>(string name, T initialValue){
        UM2_Variables thisScript = UM2_Variables.instance;
        
        //make sure there arent name duplicates (remove this in the future)
        foreach(NetworkVariable_Client networkVariable in thisScript.networkVariables){
            if(networkVariable.name == name){
                Debug.LogWarning("A server variable with name " + name + " already exists");
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

    public IEnumerator createNetworkVariable(string name, object value, Type type, int linkedID = -1){
        //wait until there are reserved IDs for variable before creating
        yield return new WaitUntil(() => reservedVariableIDs.Count > 0);

        try{
            int id = reservedVariableIDs[0];
            reservedVariableIDs.RemoveAt(0);

            NetworkVariable_Client newVariable = new NetworkVariable_Client(name, value, type, id, linkedID);
        }
        catch(Exception e){
            Debug.LogWarning("Variable failed to be created, trying again. Error message: " + e.Message);
            StartCoroutine(createNetworkVariable(name, value, type, linkedID));
        }

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
    string value;
    Type type;
    public int id;
    UM2_Variables variables;
    Action callback;

    public int linkedID; //for object based variable only, it will stay -1 if it isn't an object based variable

    public NetworkVariable_Client(string name, object value, Type type, int id, int linkedID = -1, Action callbackOnChange = null){//, Action<string> callback = null){
        //Debug.Log("Created network variable. Info:\nName: " + name + "\nType: " + type + "\nID: " + id + "\nLinked ID: " + linkedID + "\n\n");

        this.name = name;
        this.id = id;
        this.value = value + "";
        this.type = type;
        this.linkedID = linkedID;

        this.callback = callbackOnChange;

        UM2_Variables.instance.networkVariables.Add(this);
        UM2_Methods.networkMethodServer("newVar", name, id, value, type, linkedID);
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

    public void setValue(object newValue, bool sync = true){
        value = newValue + "";

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