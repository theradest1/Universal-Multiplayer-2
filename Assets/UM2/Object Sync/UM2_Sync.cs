using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System;
using System.Threading.Tasks;
using Unity.VisualScripting;

public class UM2_Sync : MonoBehaviourUM2
{
    List<GameObject> prefabs = new List<GameObject>();
    List<UM2_Object> clientSideObjects = new List<UM2_Object>();

    [Tooltip("Put any prefabs or other stuff that will be used by UM2 in this folder (default is Assets/UM2/Object Sync/Resources)")]
    public string prefabFolderPath = "Assets/UM2/Object Sync/Resources";
    List<UM2_Prefab> syncedObjects = new List<UM2_Prefab>();
    UM2_Client client;
    public static UM2_Sync sync;
    List<int> reservedIDs = new List<int>();

    [Tooltip("Each synced object needs an ID - these IDs need to be gotten from the server so there isnt any overwriting of synced objects going on. Instead of waiting for the server to respond every time we create an object, this queues up new object IDs (this variable is how many will try to be queued)")]
    public int targetPreppedObjects = 10;

    private void Awake()
    {
        sync = this;

        //loads all prefabs into a list
        UnityEngine.Object[] prefabFiles = Resources.LoadAll("", typeof(GameObject));
        foreach (UnityEngine.Object prefabFile in prefabFiles)
        {
            if(prefabFile.GetType() == typeof(GameObject)){
                prefabs.Add((GameObject)prefabFile);
            }
        }
    }

    async void reserveIDLoop(){
        await Task.Delay(100);

        if(!UM2_Client.connectedToServer){
            return;
        }

        if(reservedIDs.Count < targetPreppedObjects){
            getReserveNewObjectID();
        }

        reserveIDLoop();
    }

    private void Start()
    {
        client = gameObject.GetComponent<UM2_Client>();
        
        StartCoroutine(checkIfAllObjectsExist());

        reserveIDLoop();
    }

    public void requestVariableCreation(int objectID, int variableID, int clientID){
        UM2_Prefab variableParent_prefab = getSyncedObject(objectID, true);
        UM2_Object variableParent_object = getLocalSyncedObject(objectID, true);
        SyncedObjectVariable objectVariable = null;

        if(variableParent_prefab != null){
            objectVariable = variableParent_object.getVariable(variableID);
        }
        else if(variableParent_object != null){
            objectVariable = variableParent_object.getVariable(variableID);
        }

        
        if(objectVariable != null){
            UM2_Methods.networkMethodDirect(clientID, "createNewObjectVar", objectVariable.name, objectVariable.type, objectVariable.value, objectID, variableID);
        }
        else{
            Debug.LogWarning("Did not have requested variable: " + variableID + " on object " + objectID);
        }
    }

    public void createNewObjectVar(string name, Type type, object value, int objectID, int variableID){
        UM2_Prefab variableParent_prefab = getSyncedObject(objectID, true);
        UM2_Object variableParent_object = getLocalSyncedObject(objectID, true);

        if(variableParent_prefab != null){
            variableParent_prefab.syncNewVariable(name, type, value, variableID);
            Debug.Log("Created a new variable " + name + " on synced object " + objectID);
        }
        else if(variableParent_object != null){
            variableParent_object.syncNewVariable(name, type, value, variableID);
            Debug.Log("Created a new variable " + name + " on local object " + objectID);
        }
        else{
            Debug.LogError("Could not find object to create a variable for: " + objectID);
        }
    }

    public void setObjVar(int objectID, int variableID, string value){
        UM2_Prefab variableParent_prefab = getSyncedObject(objectID, true);
        UM2_Object variableParent_object = getLocalSyncedObject(objectID, true);

        if(variableParent_prefab != null){
            object variableValue = variableParent_prefab.getVariableValue(variableID);
            if(variableValue != null){
                variableParent_prefab.setVariableValue(variableID, value);
            }
            else{
                UM2_Methods.networkMethodOthers("requestVariableCreation", objectID, variableID, UM2_Client.clientID);
                Debug.Log("Object variable hasn't been created yet, requesting");
            }
        }
        else if(variableParent_object != null){
            object variableValue = variableParent_object.getVariableValue(variableID);
            if(variableValue != null){
                variableParent_object.setVariableValue(variableID, value);
            }
            else{
                UM2_Methods.networkMethodOthers("requestVariableCreation", objectID, variableID, UM2_Client.clientID);
                Debug.Log("Object variable hasn't been created yet, requesting");
            }
        }
        else{
            Debug.LogError("Could not find local or synced object with ID " + objectID + " for setting a variable");
        }
    }

    public async void createSyncedObject(UM2_Object startedObject){
        clientSideObjects.Add(startedObject);

        int prefabID = prefabs.IndexOf(startedObject.prefab);
        if(prefabID == -1){
            Debug.LogError("Prefab with name " + startedObject.prefab.name + " not found. Make sure to put in the folder referenced by the UM2_Sync script");
            return;
        }

        while(reservedIDs.Count == 0){
            getReserveNewObjectID();
            await Task.Delay(200);
        }

        startedObject.objectID = reservedIDs[0];
        //Debug.Log("used reserved ID " + startedObject.objectID);
        reservedIDs.RemoveAt(0);
        
        UM2_Methods.networkMethodOthers("newSyncedObject", startedObject.objectID, prefabID, startedObject.ticksPerSecond, startedObject.transform.position, startedObject.transform.rotation, UM2_Client.clientID, startedObject.destroyWhenCreatorLeaves);
    }

    public void createQuickObject(GameObject prefab, Vector3 position, Quaternion rotation, bool createLocally = true){
        int prefabID = prefabs.IndexOf(prefab);
        if(prefabID == -1){
            Debug.LogError("Could not find " + prefab + " in the resources folder");
            return;
        }
        if(createLocally){
            UM2_Methods.networkMethodGlobal("newQuickObject", prefabID, position, rotation);
        }
        else{
            UM2_Methods.networkMethodOthers("newQuickObject", prefabID, position, rotation);
        }
    }

    public void updateObject(int objectID, Vector3 position, Quaternion rotation){
        string message = "others~updateObjectTransform~" + objectID + "~" + position + "~" + rotation;
        UM2_Client.client.sendMessage(message, false, false);
    }

    public void updateTPS(int objectID, float newTPS){
        UM2_Methods.networkMethodOthers("updateObjectTPS", objectID, newTPS);
    }

    public void updateObjectTransform(int objectID, Vector3 position, Quaternion rotation){
        getSyncedObject(objectID).newTransform(position, rotation);
    }

    public void updateObjectTPS(int objectID, float newTPS){
        getSyncedObject(objectID).setTPS(newTPS);
    }

    public void newSyncedObject(int objectID, int prefabID, float ticksPerSecond, Vector3 position, Quaternion rotation, int creatorID, bool destroyOnCreatorLeave){
        //Debug.Log("Made a new synced object: " + objectID);
        
        UM2_Prefab newPrefab = GameObject.Instantiate(prefabs[prefabID].gameObject, position, rotation).AddComponent<UM2_Prefab>(); 
        syncedObjects.Add(newPrefab);
        newPrefab.initialize(objectID, ticksPerSecond, position, rotation, creatorID, destroyOnCreatorLeave);
    }

    public void destroySyncedObject(UM2_Object objectToRemove){
        clientSideObjects.Remove(objectToRemove);
        UM2_Methods.networkMethodOthers("removeSyncedObject", objectToRemove.objectID);
    }

    public void removeSyncedObject(int objectID){
        UM2_Prefab syncedObject = getSyncedObject(objectID);
        syncedObjects.Remove(syncedObject);
        Destroy(syncedObject.gameObject);
    }

    public UM2_Prefab getSyncedObject(int objectID, bool supressError = false){
        foreach(UM2_Prefab syncedObject in syncedObjects){
            if(syncedObject.objectID == objectID){
                return syncedObject;
            }
        }

        if(!supressError){
            throw new Exception("Could not find synced object with ID " + objectID + "\nThis can sometimes just happen because an update message got in front of a create object message, start to panic if it keeps going");
        }
        return null;
    }

    public void newQuickObject(int prefabID, Vector3 position, Quaternion rotation){
        GameObject.Instantiate(prefabs[prefabID].gameObject, position, rotation);
    }

    void getReserveNewObjectID(){
        UM2_Methods.networkMethodServer("reserveObjectID");
        //client.messageServer("reserveObjectID");
    }

    public void reservedObjectID(int newReservedID){
        reservedIDs.Add(newReservedID);
        //Debug.Log("reserved ID " + newReservedID);
    }

    public void giveAllSyncedObjects(int requestingClientID){
        foreach(UM2_Object clientSideObject in clientSideObjects){
            int prefabID = prefabs.IndexOf(clientSideObject.prefab);
            UM2_Methods.networkMethodOthers("newSyncedObject", clientSideObject.objectID, prefabID, clientSideObject.ticksPerSecond, clientSideObject.transform.position, clientSideObject.transform.rotation, UM2_Client.clientID, clientSideObject.destroyWhenCreatorLeaves);
        }
    }

    public UM2_Object getLocalSyncedObject(int objectID, bool supressError = false){
        foreach(UM2_Object localObject in clientSideObjects){
            if(localObject.objectID == objectID){
                return localObject;
            }
        }
        if(!supressError){
            Debug.LogError("Could not find local synced object with ID " + objectID);
        }
        return null;
    }

    IEnumerator checkIfAllObjectsExist(){ 
        //wait untill this client has an ID
        yield return new WaitUntil(() => UM2_Client.clientID != -1);

        UM2_Methods.networkMethodOthers("giveAllSyncedObjects", UM2_Client.clientID);

        yield return null;
    }
}
