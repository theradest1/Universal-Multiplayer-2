using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectSpawnExample : MonoBehaviour
{
    UM2_Sync sync;
    public GameObject cubePrefab;
    public GameObject quickObjectPrefab;
    UM2_Object selfObject;

    void Start()
    {
        sync = UM2_Sync.sync;

        selfObject = this.GetComponent<UM2_Object>();

        //selfObject.createNewVariable<int>("health", 100);
        //selfObject.createNewVariable<int>("timer", 0);

        InvokeRepeating("timerUpdate", 1, .5f);
    }

    void timerUpdate(){
        //selfObject.setVariableValue("timer", Time.time);
    }

    void Update()
    {
        if(Input.GetKeyDown("e")){
            Debug.Log("Spawning synced object");
            GameObject cube = Instantiate(cubePrefab, transform.position, transform.rotation);

        }
        if(Input.GetKeyDown("q")){
            Debug.Log("Spawning quick object"); 
            UM2_Sync.sync.createQuickObject(quickObjectPrefab, transform.position, transform.rotation);
        }
    }
}
