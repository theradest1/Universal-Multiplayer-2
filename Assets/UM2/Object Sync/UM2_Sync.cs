using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class UM2_Sync : MonoBehaviour
{
    int currentObjectID = -1;
    List<GameObject> prefabs = new List<GameObject>();
    public string prefabFolderPath;
    List<UM2_Prefab> syncedObjects = new List<UM2_Prefab>();
    UM2_Client client;

    private void Start()
    {
        //loads all prefabs into a list
        string[] prefabFiles = Directory.GetFiles(prefabFolderPath, "*.prefab"); //get all prefabs in directory
        foreach (string prefabFile in prefabFiles)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabFile); //load prefab

            if (prefab != null)
            {
                prefabs.Add(prefab); //add to list
            }
        }

        client = GetComponent<UM2_Client>();
    }

    public void createSyncedObject(UM2_Object startedObject){
        int prefabID = prefabs.IndexOf(startedObject.prefab);
        currentObjectID++;
        startedObject.objectID = currentObjectID;
        
        client.messageAllClients("newSyncedObject~" + currentObjectID);
    }

    public void updateObject(int objectID, Vector3 position, Quaternion rotation){
        client.messageAllClients("updateObjectTransform~" + objectID + "~" + position + "~" + rotation);
    }

    public void updateObjectTransform(int objectID, Vector3 position, Quaternion rotation){
        foreach(UM2_Prefab prefab in syncedObjects){
            if(prefab.objectID == objectID){
                prefab.transform.position = position;
                prefab.transform.rotation = rotation;

                return;
            }
        }

        Debug.LogError("Could not find synced object with ID " + objectID);
    }

    public void newSyncedObject(int objectID, int prefabID){
        Debug.Log("Made a new synced object: " + objectID);
        currentObjectID = objectID;

        UM2_Prefab newPrefab = GameObject.Instantiate(prefabs[prefabID].gameObject).AddComponent<UM2_Prefab>(); 
        syncedObjects.Add(newPrefab);
        newPrefab.objectID = objectID;
    }
}
