using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class UM2_Movement : MonoBehaviour
{
    UM2_Object parentScript;

    public void initialize(UM2_Object _parentScript){
        parentScript = _parentScript;
    }

    public void checkSync(){
        //parentScript.send
    }
}   
