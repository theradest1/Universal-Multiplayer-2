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
        PrintListContents(prefabs);
    }

    void PrintListContents<T>(List<T> list)
    {
        foreach (T element in list)
        {
            Debug.Log(element);
        }
    }

    public int createSyncedObject(UM2_Object startedObject){
        int prefabID = prefabs.IndexOf(startedObject.prefab);
        currentObjectID++;
        return currentObjectID;
    }

    public void newSyncedObject(int objectID, int prefabID){
        Debug.Log("Made a new synced object: " + objectID);
        currentObjectID = objectID;

        UM2_Prefab newPrefab = GameObject.Instantiate(prefabs[prefabID].gameObject).AddComponent<UM2_Prefab>(); 
        syncedObjects.Add(newPrefab);
        newPrefab.objectID = prefabID;
    }
}
