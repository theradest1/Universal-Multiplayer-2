using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;

public class UM2_Object : MonoBehaviourUM2
{

    //List<SyncedObjectVariable> syncedObjectVariables = new List<SyncedObjectVariable>();
    //[SerializeField] List<string> syncedObjectVariableNames = new List<string>();

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

    public object getNetworkVariableValue(string name){
        return UM2_Variables.getNetworkVariable(name, objectID).getValue();
    }

    public NetworkVariable_Client getNetworkVariable(string name){
        return UM2_Variables.getNetworkVariable(name, objectID);
    }

    public void setNetworkVariableValue(string name, object value){
        UM2_Variables.getNetworkVariable(name, objectID).setValue(value);
    }

    public void addToNetworkVariableValue(string name, object valueToAdd){
        UM2_Variables.getNetworkVariable(name, objectID).addToValue(valueToAdd);
    }

    private void Start()
    {
        sync = UM2_Sync.instance;
		variables = UM2_Variables.instance;
        initialize();
    }

    void OnDestroy()
    {
        sync.destroySyncedObject(this);
    }

    async void initialize(){
        //wait until client has an ID
        while (UM2_Client.clientID == -1){
            await Task.Delay(50);

            if(!UM2_Client.connectedToServer){
                return;
            }
        }

        //start syncing
        sync.createSyncedObject(this);

        //get a list of all scripts on this game object
        List<object> scripts = new List<object>();
        scripts.AddRange(gameObject.GetComponents<MonoBehaviour>());
        scripts.AddRange(gameObject.GetComponents<MonoBehaviourUM2>());

        foreach(object script in scripts){
            FieldInfo[] fields = script.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (FieldInfo field in fields)
            {
                if (Attribute.IsDefined(field, typeof(ObjectNetworkVariableAttribute)))
                {
                    //check if it is an allowed variable type
                    if(UM2_Variables.instance.allowedVariableTypes.Contains(field.FieldType)){
                        //create the variable
                        StartCoroutine(UM2_Variables.instance.createNetworkVariable(field.Name, field.GetValue(script), field.FieldType, objectID));
                    }
                    else{
                        Debug.LogError("Network variable " + field.Name + " of " + gameObject.name + " cannot be " + field.FieldType);
                    }
                }
            }
        }


        //letting other processes know they can start
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
