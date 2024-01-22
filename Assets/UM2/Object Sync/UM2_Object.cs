using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class UM2_Object : MonoBehaviour
{
    public GameObject prefab;
    [HideInInspector] public int objectID = -1;

    float pastTicksPerSecond = -1;
    [Range(1,64)]
    public float ticksPerSecond;
    UM2_Sync sync;
	UM2_Variables variables;

    public bool syncTransform = true;
    public bool optimizeTransoformSync = true;
    [Range(0, 16)]
    public float minTicksPerSecond;
    float pastSyncTime = 0;
    bool pastSyncTransform = false;
    bool initialized = false;

    Vector3 pastSyncedPos;
    Quaternion pastSyncedRot;

    public bool destroyWhenCreatorLeaves = false;

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

    public async void updateTransform(bool forced = false){
        if(this != null && syncTransform){
            if(pastTicksPerSecond != ticksPerSecond){
                pastTicksPerSecond = ticksPerSecond;
                sync.updateTPS(objectID, ticksPerSecond);
            }
            
            bool transformChanged = pastSyncedPos != transform.position || pastSyncedRot != transform.rotation;
            
            bool isMinUpdateRate = false;
            if(minTicksPerSecond > 0){
                float minTPSTime = 1/minTicksPerSecond;
                isMinUpdateRate = minTicksPerSecond <= Time.time - pastSyncTime;
            }
            
            if(transformChanged || !optimizeTransoformSync || forced || isMinUpdateRate){
                sync.updateObject(objectID, transform.position, transform.rotation);
                pastSyncedPos = transform.position;
                pastSyncedRot = transform.rotation;
                pastSyncTime = Time.time;
            }

            await Task.Delay((int)(1/ticksPerSecond*1000));
            updateTransform();
        }
    }
}
