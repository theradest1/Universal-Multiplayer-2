using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UM2_Sync : MonoBehaviour
{
    int currentObjectID = -1;
    public List<UM2_Prefab> prefabs;
    List<UM2_Prefab> syncedObjects = new List<UM2_Prefab>();

    public int startObjectSync(UM2_Object startedObject){
        int prefabID = prefabs.IndexOf(startedObject.prefab);

        currentObjectID++;
        return currentObjectID;
    }

    public void createSyncedObject(int objectID, int prefabID){
        currentObjectID = objectID;

        UM2_Prefab newPrefab = GameObject.Instantiate(prefabs[prefabID].gameObject).GetComponent<UM2_Prefab>();
        syncedObjects.Add(newPrefab);
        newPrefab.objectID = prefabID;
    }
}
