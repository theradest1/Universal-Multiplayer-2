using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

public class SyncedObjectVariable{
    public Type type;
    public object value;
    int objectID;
    public int variableID; //object relative
    public string name;


    public SyncedObjectVariable(Type _type, object _value, int _objectID, int _variableID, string _name){
        type = _type;
        value = _value;
        objectID = _objectID;
        variableID = _variableID;
        name = _name;
    }

    public void sendValue(){
        UM2_Methods.networkMethodOthers("setObjVar", objectID, variableID, value);
    }

    public void setValue(object _value, bool syncWithOthers = true){
        value = _value;
        if(syncWithOthers){
            sendValue();
        }
    }
}

public class UM2_Object : MonoBehaviourUM2
{

    List<SyncedObjectVariable> syncedObjectVariables = new List<SyncedObjectVariable>();
    [SerializeField] List<string> syncedObjectVariableNames = new List<string>();

    public GameObject prefab;
    
    public int objectID = -1;

    float pastTicksPerSecond = -1;
    [Range(1,64)]
    public float ticksPerSecond;
    UM2_Sync sync;
	UM2_Variables variables;

    public bool syncTransform = true;
    public bool optimizeTransoformSync = true;
    [Range(0, 16)]
    public float minTicksPerSecond;
    float pastSyncTime = 0;
    bool pastSyncTransform = false;
    bool initialized = false;

    Vector3 pastSyncedPos;
    Quaternion pastSyncedRot;

    public bool destroyWhenCreatorLeaves = false;

    private void Start()
    {
        sync = UM2_Sync.sync;
		variables = UM2_Variables.instance;
        initialize();
    }

    void OnDestroy()
    {
        sync.destroySyncedObject(this);
    }

    //this is when this client wants to make a new variable on this object
    public async void createNewVariable<T>(string variableName, T initialValue){
        int variableID = syncedObjectVariables.Count; // hopefully this won't backfire (:
        
        while(objectID == -1){
            await Task.Delay(50);
            Debug.Log("Waiting");
        }
        
        SyncedObjectVariable newVariable = new SyncedObjectVariable(initialValue.GetType(), initialValue, objectID, variableID, variableName);
        
        syncedObjectVariableNames.Add(variableName);
        syncedObjectVariables.Add(newVariable);
        
        newVariable.sendValue();
    }

    public SyncedObjectVariable getVariable(string variableName){
        return getVariable(syncedObjectVariableNames.IndexOf(variableName));
    }

    public SyncedObjectVariable getVariable(int variableID){
        return syncedObjectVariables[variableID];
    }

    //this is when others make a new variable for this object
    public void syncNewVariable(string variableName, Type type, object initialValue, int variableID){
        SyncedObjectVariable newVariable = new SyncedObjectVariable(type, initialValue, objectID, variableID, variableName);
        
        syncedObjectVariableNames.Add(variableName);
        syncedObjectVariables.Add(newVariable);
    }

    public object getVariableValue(string variableName){
        return getVariableValue(syncedObjectVariableNames.IndexOf(variableName));
    }

    public object getVariableValue(int variableID){
        try{
            return syncedObjectVariables[variableID].value;
        }
        catch{
            return null;
        }
    }

    public void setVariableValue(string variableName, object value){
        setVariableValue(syncedObjectVariableNames.IndexOf(variableName), value);
    }

    public void setVariableValue(int variableID, object value){
        syncedObjectVariables[variableID].setValue(value, false);
    }


    async void initialize(){
        while (UM2_Client.clientID == -1){
            await Task.Delay(50);

            if(!UM2_Client.connectedToServer){
                return;
            }
        }
        sync.createSyncedObject(this);
        initialized = true;
    }

    private void Update()
    {
        if(initialized){ // if object is being synced
            if(pastSyncTransform != syncTransform){ //if it has been changed
                pastSyncTransform = syncTransform;
                if(syncTransform){
                    updateTransform();
                }
            }
        }
    }

    public async void updateTransform(bool forced = false){
        if(this != null && syncTransform){
            if(pastTicksPerSecond != ticksPerSecond){
                pastTicksPerSecond = ticksPerSecond;
                sync.updateTPS(objectID, ticksPerSecond);
            }
            
            bool transformChanged = pastSyncedPos != transform.position || pastSyncedRot != transform.rotation;
            
            bool isMinUpdateRate = false;
            if(minTicksPerSecond > 0){
                float minTPSTime = 1/minTicksPerSecond;
                isMinUpdateRate = minTicksPerSecond <= Time.time - pastSyncTime;
            }
            
            if(transformChanged || !optimizeTransoformSync || forced || isMinUpdateRate){
                sync.updateObject(objectID, transform.position, transform.rotation);
                pastSyncedPos = transform.position;
                pastSyncedRot = transform.rotation;
                pastSyncTime = Time.time;
            }

            await Task.Delay((int)(1/ticksPerSecond*1000));
            updateTransform();
        }
    }
}
