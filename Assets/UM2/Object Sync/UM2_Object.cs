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

    private void Start()
    {
        sync = UM2_Sync.sync;
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
        updateTransform();
    }

    async void updateTransform(){
        if(this != null){
            sync.updateObject(objectID, transform.position, transform.rotation);

            await Task.Delay((int)(1/ticksPerSecond*1000));
            updateTransform();
        }
    }
}
