using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MainMenu : MonoBehaviour
{
    public TMP_InputField serverIpInput;
    public TMP_InputField serverPortInput;
    public Toggle hostServerToggle;

    public static bool hosting = false;

    public void Join()
    {
        SceneManager.LoadScene("Game");
    }

    public void updateThings()
    {
        try
        {
            UM2_Client.serverIP = serverIpInput.text;
            UM2_Client.serverUdpPort = int.Parse(serverPortInput.text);
            UM2_Client.serverTcpPort = int.Parse(serverPortInput.text) + 1;
            UM2_Client.serverHttpPort = int.Parse(serverPortInput.text) + 2;
            UM2_Client.hostingServer = hostServerToggle.isOn;
        }
        catch
        {
        }
    }
}
