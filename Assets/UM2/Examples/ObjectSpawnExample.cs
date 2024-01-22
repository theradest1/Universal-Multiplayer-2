using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectSpawnExample : MonoBehaviour
{
    UM2_Sync sync;
    public GameObject cubePrefab;

    void Start()
    {
        sync = UM2_Sync.sync;
    }

    void Update()
    {
        if(Input.GetKeyDown("e")){
            Debug.Log("Spawning object");
            Instantiate(cubePrefab, transform.position, transform.rotation);
        }
    }
}
