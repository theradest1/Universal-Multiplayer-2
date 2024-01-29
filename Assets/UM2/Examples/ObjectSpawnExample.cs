using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectSpawnExample : MonoBehaviour
{
    UM2_Sync sync;
    public GameObject cubePrefab;
    public GameObject quickObjectPrefab;

    void Start()
    {
        sync = UM2_Sync.sync;
    }

    void Update()
    {
        if(Input.GetKeyDown("e")){
            Debug.Log("Spawning synced object");
            Instantiate(cubePrefab, transform.position, transform.rotation);
        }
        if(Input.GetKeyDown("q")){
            Debug.Log("Spawning quick object");
            UM2_Sync.sync.createQuickObject(quickObjectPrefab, transform.position, transform.rotation);
        }
    }
}
