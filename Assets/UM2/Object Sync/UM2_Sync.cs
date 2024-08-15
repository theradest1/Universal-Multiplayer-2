using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System;
using System.Threading.Tasks;

public class UM2_Sync : MonoBehaviourUM2
{
    List<GameObject> prefabs = new List<GameObject>();  
    List<UM2_Object> clientSideObjects = new List<UM2_Object>();
    List<UM2_Prefab> syncedObjects = new List<UM2_Prefab>();
    public static UM2_Sync instance;
    List<int> reservedIDs = new List<int>();

    public int targetPreppedObjects = 10;

    private void Awake()
    {
        instance = this;

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
        StartCoroutine(checkIfAllObjectsExist());

        reserveIDLoop();
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
        reservedIDs.RemoveAt(0);
        
        UM2_Methods.networkMethodOthers("newSyncedObject", startedObject.objectID, prefabID, startedObject.ticksPerSecond, startedObject.transform.position, startedObject.transform.rotation, UM2_Client.clientID);
    }

    public static void createQuickObject(GameObject prefab, Vector3 position, Quaternion rotation){
        int prefabID = UM2_Sync.instance.prefabs.IndexOf(prefab);
        if(prefabID == -1){
            Debug.LogError("Could not find " + prefab + " in the resources folder, go read the docs if you don't know what this means");
            return;
        }
        else{
            UM2_Methods.networkMethodGlobal("newQuickObject", prefabID, position, rotation);
        }
    }

    public void sendUpdateObjectTransform(int objectID, Vector3 position, Quaternion rotation, bool ease){
        string message = "others~updateObjectTransform~" + objectID + "~" + position + "~" + rotation + "~" + ease;
        UM2_Client.instance.sendMessage(message, false, false);
    }

    public void updateObject(int objectID, string parameters){
        string message = "others~updateObjectAnimation~" + objectID + "~" + parameters;
        UM2_Client.instance.sendMessage(message, false, false);
    }

    public void updateTPS(int objectID, float newTPS){
        UM2_Methods.networkMethodOthers("updateObjectTPS", objectID, newTPS);
    }

    public void updateObjectTransform(int objectID, Vector3 position, Quaternion rotation, bool ease){
        UM2_Prefab prefab = getSyncedObject(objectID, true);
        if(prefab != null){
            prefab.newTransform(position, rotation, ease);
        }
        else{
            UM2_Methods.networkMethodOthers("giveSyncedObject", UM2_Client.clientID, objectID);
        }
    }

    public void updateObjectAnimation(int objectID, string parameters){
        UM2_Prefab prefab = getSyncedObject(objectID, true);
        if(prefab != null){
            prefab.newAnimationParameters(parameters);
        }
        else{
            UM2_Methods.networkMethodOthers("giveSyncedObject", UM2_Client.clientID, objectID);
        }
    }

    public void updateObjectTPS(int objectID, float newTPS){
        UM2_Prefab prefab = getSyncedObject(objectID, true);
        if(prefab != null){
            prefab.setTPS(newTPS);
        }
        else{
            UM2_Methods.networkMethodOthers("giveSyncedObject", UM2_Client.clientID, objectID);
        }
    }

    public void newSyncedObject(int objectID, int prefabID, float ticksPerSecond, Vector3 position, Quaternion rotation, int creatorID){
        //Debug.Log("Made a new synced object: " + objectID);
        if(getSyncedObject(objectID, true) == null && getLocalSyncedObject(objectID, true) == null){
            //spawn prefab
            GameObject newPrefab = GameObject.Instantiate(prefabs[prefabID].gameObject, position, rotation);

            //get prefab script
            UM2_Prefab newPrefabScript = newPrefab.GetComponent<UM2_Prefab>();

            //if there isnt one, add one
            if(newPrefabScript == null){
                newPrefabScript = newPrefab.AddComponent<UM2_Prefab>();
            }

            //add info
            syncedObjects.Add(newPrefabScript);
            newPrefabScript.initialize(objectID, ticksPerSecond, position, rotation, creatorID);
        }
        else{
            if(UM2_Client.instance.debugBasicMessages){
                Debug.LogWarning("Synced object with ID " + objectID + " already exists, ignoring creation");
            } 
        }
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
            UM2_Methods.networkMethodDirect("newSyncedObject", requestingClientID, clientSideObject.objectID, prefabID, clientSideObject.ticksPerSecond, clientSideObject.transform.position, clientSideObject.transform.rotation, UM2_Client.clientID);
        }
    }

    public void giveSyncedObject(int requestingClientID, int syncedObjectID){
        //Debug.Log("Client with ID " + requestingClientID + " asked for synced object with ID " + syncedObjectID);
        UM2_Object clientSideObject = getLocalSyncedObject(syncedObjectID, true);
        if(clientSideObject != null){
            int prefabID = prefabs.IndexOf(clientSideObject.prefab);
            UM2_Methods.networkMethodDirect("newSyncedObject", requestingClientID, clientSideObject.objectID, prefabID, clientSideObject.ticksPerSecond, clientSideObject.transform.position, clientSideObject.transform.rotation, UM2_Client.clientID);
            return;
        }
        //Debug.LogError("Another client asked for synced object with id " + syncedObjectID + ", but it doesnt exist here either ):");
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
