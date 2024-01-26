using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Debugger : MonoBehaviour
{
    public GameObject debugParent;
    List<TextMeshProUGUI> debugTexts = new List<TextMeshProUGUI>();
    List<string> titles = new List<string>();
    public GameObject debugTextPrefab;
    public static Debugger debugger;
    UM2_Client client;

    private void Awake() {
        debugger = this;
    }

    private void Start() {
        client = UM2_Client.client;

        debugger.addSpace("Client:");
        debugger.setDebug("UDP", "ms B/s↑ B/s↓");
        debugger.setDebug("TCP", "ms B/s↑ B/s↓");
        debugger.setDebug("HTTP", "ms B/s↑ B/s↓");
        debugger.setDebug("Failed/Sec", "0");

        if (UM2_Client.hostingServer)
        {
            debugger.addSpace();
            debugger.addSpace("Server:");
            debugger.setDebug(" UDP", "offline B/s↑ B/s↓");
            debugger.setDebug(" TCP", "offline B/s↑ B/s↓");
            debugger.setDebug(" HTTP", "offline B/s↑ B/s↓");
            debugger.setDebug(" Failed/Sec ", "0");
        }
        InvokeRepeating("updateDebug", 1f, 1f);
    }

    public void setDebug(string title, string value)
    {
        int debugID = titles.IndexOf(title);

        if (debugID == -1)
        {
            TextMeshProUGUI newText = GameObject.Instantiate(debugTextPrefab, debugParent.transform).GetComponent<TextMeshProUGUI>();
            newText.name = title + " debug";

            debugTexts.Add(newText);
            titles.Add(title);
            setDebug(title, value);
            return;
        }

        debugTexts[debugID].text = title + ": " + value;
    }

    public void addSpace(string title = " ")
    {
        if (debugTexts.Count != 0 || title != " ")
        {
            TextMeshProUGUI newText = GameObject.Instantiate(debugTextPrefab, debugParent.transform).GetComponent<TextMeshProUGUI>();
            newText.name = "spacer";
            newText.text = title;

            debugTexts.Add(newText);
            titles.Add("space");
        }
    }

    void updateDebug()
    {
        setDebug("UDP", $"{(int)(client.udpPing * 1000)}ms  {client.sentBytesUDP}B/s↑  {client.gotBytesUDP}B/s↓");
        setDebug("TCP", $"{(int)(client.tcpPing * 1000)}ms  {client.sentBytesTCP}B/s↑  {client.gotBytesTCP}B/s↓");
        setDebug("HTTP", $"{(int)(client.httpPing * 1000)}ms  {client.sentBytesHTTP}B/s↑  {client.gotBytesHTTP}B/s↓");
        setDebug("Failed/Sec", client.failedMessages + "");

        client.sentBytesUDP = 0;
        client.gotBytesUDP = 0;
        client.sentBytesTCP = 0;
        client.gotBytesTCP = 0;
        client.sentBytesHTTP = 0;
        client.gotBytesHTTP = 0;
        client.failedMessages = 0;

        //sendDebugMessage("test");
    }
}
