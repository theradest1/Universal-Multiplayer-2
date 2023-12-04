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

    private void Start()
    {
        string ip = PlayerPrefs.GetString("serverIP");
        string port = PlayerPrefs.GetString("serverPort");
        bool server = bool.Parse(PlayerPrefs.GetString("hostServer"));

        serverIpInput.text = ip;
        serverPortInput.text = port;
        hostServerToggle.isOn = server;
    }

    public void Join()
    {
        saveSettings();
        SceneManager.LoadScene("Game");
    }

    public void saveSettings()
    {
        Debug.Log("Saved");
        PlayerPrefs.SetString("serverIP", serverIpInput.text);
        PlayerPrefs.SetString("serverPort", serverPortInput.text);
        PlayerPrefs.SetString("hostServer", hostServerToggle.isOn + "");

        PlayerPrefs.Save();
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

            saveSettings();
        }
        catch
        {
        }
    }
}
