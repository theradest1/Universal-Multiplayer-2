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
    
    [HideInInspector] public int objectID = -1;

    float pastTicksPerSecond = -1;
    [Range(1,64)]
    public float ticksPerSecond = 20;
    UM2_Sync sync;

    public bool syncTransform = true;
    public bool optimizeTransoformSync = true;
    [Range(0, 16)]
    public float minTicksPerSecond = 0;
    float pastSyncTime = 0;
    bool pastSyncTransform = false;
    [HideInInspector] public bool initialized = false;

    Vector3 pastSyncedPos;
    Quaternion pastSyncedRot;

    bool skipNextTransformEase = false;

    public T getNetworkVariableValue<T>(string name){
        NetworkVariable_Client networkVariable = getNetworkVariable(name);
        if(typeof(T) != networkVariable.type){
            Debug.LogWarning("Network variable \"" + name + "\" is " + networkVariable.type + ", but you requested " + typeof(T));
        }
        return (T)networkVariable.getValue();
    }

    public void setNetworkVariableValue(string name, object value){
        getNetworkVariable(name).setValue(value);
    }

    public void addToNetworkVariableValue(string name, object valueToAdd){
        getNetworkVariable(name).addToValue(valueToAdd);
    }

    public async void addVarCallback(string name, Action<object> method){
        if(objectID == -1){
            while (objectID == -1){
                await Task.Delay(50);
                
                if(!UM2_Client.connectedToServer){
                    return;
                }
            }
        }
        
        UM2_Variables.addVarCallback(name, method, objectID);
    }

    public NetworkVariable_Client getNetworkVariable(string name){
        if(objectID == -1){
            Debug.LogWarning("Cannot get network variable since object doesn't have a variable yet\njust wait for a bit before getting it (or ignore)");
            return null;
        }

        return UM2_Variables.getNetworkVariable(name, objectID);
    }

    private void Start()
    {
        sync = UM2_Sync.instance;
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

        //wait until this object has an ID
        while (objectID == -1){
            await Task.Delay(50);

            if(!UM2_Client.connectedToServer){
                return;
            }
        }
        //get a list of all scripts on this game object
        MonoBehaviour[] scripts = gameObject.GetComponents<MonoBehaviour>();

        foreach(MonoBehaviour script in scripts){
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
        //this peice of code turns on the updateTransform loop
        //if syncTransform is set to false, the updateTransform loop stops
        //but if it is set back to true, this peice of code starts it back up again
        //it also turns it on at the start

        // if object is created on the network
        if(initialized){

            //if the syncTransform toggle was changed
            if(pastSyncTransform != syncTransform){
                pastSyncTransform = syncTransform;

                //if it was set to true, turn the loop back on
                if(syncTransform){
                    updateTransform();
                }
            }
        }
    }

    public void teleportObject(){
        skipNextTransformEase = true;
    }

    public async void updateTransform(bool forced = false){
        if(this != null && syncTransform){
            
            //if the tps is changed, sync it to other clients
            if(pastTicksPerSecond != ticksPerSecond){
                pastTicksPerSecond = ticksPerSecond;
                sync.updateTPS(objectID, ticksPerSecond);
            }
            
            //if the object has moved
            bool transformChanged = (pastSyncedPos != transform.position) || (pastSyncedRot != transform.rotation);

            bool isMinUpdateRate = (minTicksPerSecond > 0) && (1/minTicksPerSecond <= Time.time - pastSyncTime);
            
            if(transformChanged || !optimizeTransoformSync || forced || isMinUpdateRate){
                sync.sendUpdateObjectTransform(objectID, transform.position, transform.rotation, !skipNextTransformEase);
                pastSyncedPos = transform.position;
                pastSyncedRot = transform.rotation;
                pastSyncTime = Time.time;
            }

            await Task.Delay((int)(1/ticksPerSecond*1000));
            updateTransform();
        }
    }
}
