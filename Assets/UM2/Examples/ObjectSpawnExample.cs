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
            GameObject cube = Instantiate(cubePrefab, transform.position, transform.rotation);
            cube.GetComponent<UM2_Object>().createNewVariable<string>("testVar", "hola");

        }
        if(Input.GetKeyDown("q")){
            Debug.Log("Spawning quick object"); 
            UM2_Sync.sync.createQuickObject(quickObjectPrefab, transform.position, transform.rotation);
        }
    }
}
