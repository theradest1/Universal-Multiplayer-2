using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System;

public class UM2_Sync : MonoBehaviour
{
    int currentObjectID = -1;
    List<GameObject> prefabs = new List<GameObject>();
    public string prefabFolderPath;
    List<UM2_Prefab> syncedObjects = new List<UM2_Prefab>();
    UM2_Client client;
    public static UM2_Sync sync;

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

    private void Start()
    {
        client = gameObject.GetComponent<UM2_Client>();
    }

    public void createSyncedObject(UM2_Object startedObject){
        int prefabID = prefabs.IndexOf(startedObject.prefab);
        if(prefabID == -1){
            Debug.LogError("Prefab with name " + startedObject.prefab.name + " not found. Make sure to put in the folder referenced by the UM2_Sync script");
            return;
        }
        currentObjectID++;
        //Debug.Log("Creating new synced object " + currentObjectID);
        startedObject.objectID = currentObjectID;
        
        client.messageAllClients("newSyncedObject~" + currentObjectID + "~" + prefabID + "~" + startedObject.ticksPerSecond);
    }

    public void updateObject(int objectID, Vector3 position, Quaternion rotation){
        //Debug.Log("Updating object " + objectID);
        client.messageAllClients("updateObjectTransform~" + objectID + "~" + position + "~" + rotation, false);
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
        //Debug.Log("Made a new synced object: " + objectID);
        currentObjectID = objectID;

        UM2_Prefab newPrefab = GameObject.Instantiate(prefabs[prefabID].gameObject).AddComponent<UM2_Prefab>(); 
        syncedObjects.Add(newPrefab);
        newPrefab.objectID = objectID;
        //newPrefab.ticksPerSecond = ticksPerSecond;
    }
}
