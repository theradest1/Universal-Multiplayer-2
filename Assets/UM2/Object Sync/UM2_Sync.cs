using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System;
using System.Threading.Tasks;

public class UM2_Sync : MonoBehaviour
{
    List<GameObject> prefabs = new List<GameObject>();
    List<UM2_Object> clientSideObjects = new List<UM2_Object>();
    public string prefabFolderPath;
    List<UM2_Prefab> syncedObjects = new List<UM2_Prefab>();
    UM2_Client client;
    public static UM2_Sync sync;
    List<int> reservedIDs = new List<int>();

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

    public async void createSyncedObject(UM2_Object startedObject){
        clientSideObjects.Add(startedObject);

        int prefabID = prefabs.IndexOf(startedObject.prefab);
        if(prefabID == -1){
            Debug.LogError("Prefab with name " + startedObject.prefab.name + " not found. Make sure to put in the folder referenced by the UM2_Sync script");
            return;
        }

        while(reservedIDs.Count == 0){
            getReserveNewObjectID();
            await Task.Delay(50);
        }

        startedObject.objectID = reservedIDs[0];
        //Debug.Log("used reserved ID " + startedObject.objectID);
        reservedIDs.RemoveAt(0);
        
        client.messageOtherClients("newSyncedObject~" + startedObject.objectID + "~" + prefabID + "~" + startedObject.ticksPerSecond);
    }

    public void updateObject(int objectID, Vector3 position, Quaternion rotation){
        client.messageOtherClients("updateObjectTransform~" + objectID + "~" + position + "~" + rotation, false);
    }

    public void updateObjectTransform(int objectID, Vector3 position, Quaternion rotation){
        foreach(UM2_Prefab prefab in syncedObjects){
            if(prefab.objectID == objectID){
                prefab.newTransform(position, rotation);
                return;
            }
        }

        Debug.LogWarning("Could not find synced object with ID " + objectID + "\nThis can sometimes just happen because an update message got in front of a create object message, start to panic if it keeps going");
    }

    public void newSyncedObject(int objectID, int prefabID, float ticksPerSecond){
        Debug.Log("Made a new synced object: " + objectID);

        UM2_Prefab newPrefab = GameObject.Instantiate(prefabs[prefabID].gameObject).AddComponent<UM2_Prefab>(); 
        syncedObjects.Add(newPrefab);
        newPrefab.objectID = objectID;
    }

    void getReserveNewObjectID(){
        client.messageServer("reserveObjectID");
    }

    public void reservedObjectID(int newReservedID){
        reservedIDs.Add(newReservedID);
        //Debug.Log("reserved ID " + newReservedID);
    }

    public void giveAllSyncedObjects(int requestingClientID){
        foreach(UM2_Object clientSideObject in clientSideObjects){
            int prefabID = prefabs.IndexOf(clientSideObject.prefab);
            Debug.Log("newSyncedObject~" + clientSideObject.objectID + "~" + prefabID + "~" + clientSideObject.ticksPerSecond);
            client.messageToOtherClient("newSyncedObject~" + clientSideObject.objectID + "~" + prefabID + "~" + clientSideObject.ticksPerSecond, requestingClientID);
        }
    }

    IEnumerator checkIfAllObjectsExist(){
        //wait untill this client has an ID
        yield return new WaitUntil(() => UM2_Client.clientID != -1);

        client.messageOtherClients("giveAllSyncedObjects~" + UM2_Client.clientID);

        yield return null;
    }
}
