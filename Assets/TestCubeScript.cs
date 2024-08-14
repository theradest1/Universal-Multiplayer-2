using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestCubeScript : MonoBehaviour
{
    [ObjectNetworkVariable] float cubeVariable1 = 10.5f;
    [ObjectNetworkVariable] string cubeVariable2 = "this is a cube";

    UM2_Object objectScript;

    void neverCalledFunction(){
        cubeVariable1 += 0;
        cubeVariable2 += "";
    }

    void Start()
    {
        objectScript = this.GetComponent<UM2_Object>();
        objectScript.addVarCallback("cubeVariable1", callbackMethod);
        objectScript.addVarCallback("not a var", callbackMethod);
        InvokeRepeating("yuh", 1, 1);
    }

    void yuh(){
        objectScript.addToNetworkVariableValue("cubeVariable1", 1); 
    }

    public void callbackMethod(object value){
        Debug.Log("Called back :D");
    }
}
