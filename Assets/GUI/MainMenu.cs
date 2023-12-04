using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityEditor.PackageManager.UI;

public class MainMenu : MonoBehaviour
{
    public TMP_InputField serverIpInput;
    public TMP_InputField serverPortInput;
    public Toggle hostServerToggle;

    public static bool hosting = false;

    private void Start()
    {
        //doing this ahead of time so there is less waiting
        //UM2_Server.GetLocalIPAddress();
        //UM2_Server.GetPublicIPAddress();

        string ip;
        string port;
        bool server;
        try
        {
            ip = PlayerPrefs.GetString("serverIP");
            port = PlayerPrefs.GetString("serverPort");
            server = bool.Parse(PlayerPrefs.GetString("hostServer"));
        }
        catch
        {
            ip = "127.0.0.1";
            port = "5000";
            server = false;
        }

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
