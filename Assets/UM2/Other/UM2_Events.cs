using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UM2_Events : MonoBehaviourUM2
{
    UM2_Client client;
    //this is a bunch of methods the server calls

    void Start()
    {
        client = UM2_Client.client;
    }

    public void clientJoined(int newClientID){
        Debug.Log("Client with ID " + newClientID + " joined");

        UM2_Methods.callGlobalMethod("OnPlayerJoin", new object[] {newClientID});
    }

    public void clientDisconnected(int goneClientID){
        UM2_Methods.callGlobalMethod("OnPlayerLeave", new object[] {goneClientID});
    }

    public void setID(int id){
        UM2_Client.clientID = id;

        //client.sendMessage("server~saveProtocol", "UDP", false);
        //client.sendMessage("server~saveProtocol", "TCP", false);
        //nothing for http since it is only client->server->client (no disjointed response)

        UM2_Methods.callGlobalMethod("OnConnect", new object[] {id});
    }

    public void recordedProtocol(string protocol){
        Debug.Log("Recorded: " + protocol);

        if(protocol == "TCP"){
            client.tcpRecorded = true;
        }
        else if(protocol == "UDP"){
            client.udpRecorded = true;
        }
        else{
            Debug.LogError("Recorded protocol that does exist???");
        }
    }
}
