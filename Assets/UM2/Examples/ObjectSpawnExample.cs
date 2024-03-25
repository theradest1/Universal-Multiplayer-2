using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectSpawnExample : MonoBehaviour
{
    UM2_Sync sync;
    public GameObject cubePrefab;
    public GameObject quickObjectPrefab;
    UM2_Object selfObject;
    int variableName = 0;
    int cubeCount = 0;
    [ObjectNetworkVariable] int testVar1 = 103;
    [ObjectNetworkVariable] string testVar2 = "hola";

    void Start()
    {
        sync = UM2_Sync.instance;

        selfObject = this.GetComponent<UM2_Object>();
    }

    void Update()
    {
        if(Input.GetKeyDown("e")){
            Debug.Log("Spawning synced object");
            GameObject cube = Instantiate(cubePrefab, transform.position, transform.rotation);
            cube.name += cubeCount;
            cubeCount++;
        }
        if(Input.GetKeyDown("q")){
            Debug.Log("Spawning quick object"); 
            UM2_Sync.instance.createQuickObject(quickObjectPrefab, transform.position, transform.rotation);
        }
        if(Input.GetKeyDown("v")){
            Debug.Log("Creating network variable");
            variableName++;
            UM2_Variables.createNetworkVariable<float>(variableName + "", Time.time);  
        }
    }
}
