using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class UM2_Object : MonoBehaviour
{
    public GameObject prefab;
    [HideInInspector] public int objectID = -1;

    [Range(1,64)]
    public float ticksPerSecond;
    UM2_Sync sync;
	UM2_Variables variables;

    public bool syncTransform = true;
    bool pastSyncTransform = false;
    bool initialized = false;

    private void Start()
    {
        sync = UM2_Sync.sync;
		variables = UM2_Variables.instance;
        initialize();
    }

    async void initialize(){
        while (UM2_Client.clientID == -1){
            await Task.Delay(50);

            if(!UM2_Client.connectedToServer){
                return;
            }
        }
        sync.createSyncedObject(this);
        initialized = true;

		variables.createServerVariable("testvar1", "yuh");
		print(variables.getServerVariableValue("testvar1"));
    }

    private void Update()
    {
        if(initialized){ // if object is being synced
            if(pastSyncTransform != syncTransform){ //if it has been changed
                pastSyncTransform = syncTransform;
                if(syncTransform){
                    updateTransform();
                }
            }
        }
    }

    async void updateTransform(){
        if(this != null && syncTransform){
            sync.updateObject(objectID, transform.position, transform.rotation);

            await Task.Delay((int)(1/ticksPerSecond*1000));
            updateTransform();
        }
    }
}
