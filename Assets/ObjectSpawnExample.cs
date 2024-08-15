using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ObjectSpawnExample : MonoBehaviourUM2
{
    public GameObject cubePrefab;
    public GameObject quickObjectPrefab;
    public TextMeshProUGUI timerText;

    public override void OnConnect(int clientID)
    {
        //create the network variable
        UM2_Variables.createNetworkVariable<float>("Timer", 0f);
        UM2_Variables.addVarCallback("Timer", updateTimerGUI);

        //if you are the owner of the server
        if(UM2_Client.hostingServer){
            //start a loop that updates the network variable
            InvokeRepeating("updateTimerVar", .5f, .1f);
        }
    }

    void updateTimerVar(){
        //set the timer network variable to the time
        UM2_Variables.setNetworkVariableValue("Timer", (float)Time.time);
    }

    public void updateTimerGUI(object newTimerValue){
        //set the timer text to be the value of the timer network variable
        timerText.text = newTimerValue + "";
    }

    void Update()
    {
        if(Input.GetKeyDown("e")){
            Debug.Log("Spawning synced object");
            GameObject cube = Instantiate(cubePrefab, transform.position, transform.rotation);
        }
        if(Input.GetKeyDown("q")){
            Debug.Log("Spawning quick object"); 
            UM2_Sync.createQuickObject(quickObjectPrefab, transform.position, transform.rotation);
        }
    }
}
