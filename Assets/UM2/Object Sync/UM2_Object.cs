using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class UM2_Object : MonoBehaviour
{
    public GameObject prefab;
    [HideInInspector] public int objectID = -1;
    public float ticksPerSecond;
    UM2_Sync sync;

    private void Start()
    {
        sync = UM2_Sync.sync;
        initialize();
    }

    async void initialize(){
        while (UM2_Client.clientID == -1){
            await Task.Delay(200);

            if(!UM2_Client.connectedToServer){
                return;
            }
        }
        sync.createSyncedObject(this);
        InvokeRepeating("updateTransform", 0, 1/ticksPerSecond);
    }

    void updateTransform(){
        sync.updateObject(objectID, transform.position, transform.rotation);
    }
}
