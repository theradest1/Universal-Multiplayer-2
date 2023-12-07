using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UM2_Object : MonoBehaviour
{
    public GameObject prefab;
    public int objectID;
    public float ticksPerSecond;
    public UM2_Sync sync;

    private void Start()
    {
        InvokeRepeating("updateTransform", 1, 1/ticksPerSecond);
    }

    void updateTransform(){
        sync.updateObject(objectID, transform.position, transform.rotation);
    }
}
