using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using Unity.VisualScripting;
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
        string[] prefabFiles = Directory.GetFiles(prefabFolderPath, "*.prefab"); //get all prefabs in directory
        foreach (string prefabFile in prefabFiles)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabFile); //load prefab

            if (prefab != null)
            {
                prefabs.Add(prefab);
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
        Debug.Log("Creating new synced object " + currentObjectID);
        startedObject.objectID = currentObjectID;
        
        client.messageAllClients("newSyncedObject~" + currentObjectID + "~" + prefabID);
    }

    public void updateObject(int objectID, Vector3 position, Quaternion rotation){
        Debug.Log("Updating object " + objectID);
        client.messageAllClients("updateObjectTransform~" + objectID + "~" + position + "~" + rotation, false);
    }

    public void updateObjectTransform(int objectID, Vector3 position, Quaternion rotation){
        Debug.Log("Syncing object with ID " + objectID);
        foreach(UM2_Prefab prefab in syncedObjects){
            if(prefab.objectID == objectID){
                prefab.transform.position = position;
                prefab.transform.rotation = rotation;

                return;
            }
        }

        Debug.LogWarning("Could not find synced object with ID " + objectID + "\nThis can sometimes just happen because an update message got in front of a create object message. (only worry if it continues)");
    }

    public void newSyncedObject(int objectID, int prefabID){
        Debug.Log("Made a new synced object: " + objectID);
        currentObjectID = objectID;

        UM2_Prefab newPrefab = GameObject.Instantiate(prefabs[prefabID].gameObject).AddComponent<UM2_Prefab>(); 
        syncedObjects.Add(newPrefab);
        newPrefab.objectID = objectID;
    }
}
