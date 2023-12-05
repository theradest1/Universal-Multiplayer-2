using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;
using JetBrains.Annotations;
using System;
using System.Net.Sockets;

public class MainMenu : MonoBehaviour
{
    public TMP_InputField joinKeyInput;
    public Toggle hostServerToggle;
    public GameObject generateKeysButton;
    public TextMeshProUGUI keyDisplays;

    string localKey;
    string publicKey;

    public static bool hosting = false;

    //the characters allowed to be used in the join keys
    public string characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

    public void copyKey(bool copyLocalKey){
        if(copyLocalKey){
            GUIUtility.systemCopyBuffer = localKey;
        }
        else{
            GUIUtility.systemCopyBuffer = publicKey;
        }
    }

    public void pasteKey(){
        joinKeyInput.text = GUIUtility.systemCopyBuffer;
    }

    private void Start()
    {
        //does a few things like disabling the server, udp, and tcp
        #if UNITY_WEBGL && !UNITY_EDITOR
            UM2_Client.webGLBuild = true;
        #endif

        string joinKey;
        bool server;
        try
        {
            joinKey = PlayerPrefs.GetString("joinKey");
            server = bool.Parse(PlayerPrefs.GetString("hostServer"));
        }
        catch
        {
            joinKey = "";
            server = false;
        }

        joinKeyInput.text = joinKey;
        hostServerToggle.isOn = server;

        if(UM2_Client.webGLBuild){
            hostServerToggle.isOn = false;
            hostServerToggle.gameObject.SetActive(false);
        }
        else{
            UM2_Server.GetLocalIPAddress();
            UM2_Server.GetPublicIPAddress();
        }

        updateThings();
        print("Local host join key: " + encodeJoinKey("127.0.0.1"));
    }

    public void Join()
    {
        saveSettings();
        SceneManager.LoadScene("Game");
    }

    public void saveSettings()
    {
        PlayerPrefs.SetString("joinKey", joinKeyInput.text);
        PlayerPrefs.SetString("hostServer", hostServerToggle.isOn + "");

        PlayerPrefs.Save();
    }

    public void updateThings()
    {
        try
        {
            UM2_Client.hostingServer = hostServerToggle.isOn;

            //turn things on and off
            joinKeyInput.gameObject.SetActive(!UM2_Client.hostingServer);
            generateKeysButton.SetActive(UM2_Client.hostingServer);
            keyDisplays.gameObject.SetActive(UM2_Client.hostingServer);
            
            if(UM2_Client.hostingServer){
                UM2_Client.serverIP = "127.0.0.1"; //ip for local host
                generateJoinKey();
            }
            else{
                UM2_Client.serverIP = decodeJoinKey(joinKeyInput.text);
            }

            saveSettings();
        }
        catch
        {
            //this catch is just so errors dont happen when trying to get a int from an input
        }
    }

    public void generateJoinKey(){
        if(UM2_Server.localIpAddress == null || UM2_Server.publicIpAddress == null)
        {
            keyDisplays.text = "Loading...";
            Invoke("generateJoinKey", .5f);
            return;
        }

        localKey = encodeJoinKey(UM2_Server.localIpAddress);
        publicKey = encodeJoinKey(UM2_Server.publicIpAddress);

        keyDisplays.text = "LAN: " + localKey + "\nWAN: " + publicKey;
    }

    public string encodeJoinKey(string ipString)
    {
        //convert to number
        string[] chunks = ipString.Split(".");
        string finalIPString = "";
        //make each chunk 3 characters
        foreach(string chunk in chunks){
            if(chunk.Length == 3){
                finalIPString += chunk;
            }
            else{
                string newChunk = chunk;
                while(newChunk.Length < 3){
                    newChunk = "0" + newChunk;
                }
                finalIPString += newChunk;
            }
        }
        long number = long.Parse(finalIPString);
        
        //encode
        string encodedString = "";
        int baseNumber = characters.Length;
        while (number > 0)
        {
            encodedString = characters[(int)(number % baseNumber)] + encodedString;
            number /= baseNumber;
        }

        return encodedString;
    }

    public string decodeJoinKey(string encodedNumber){
        encodedNumber = encodedNumber.ToUpper();
        
        // Convert the encoded string back to the original number
        long decodedNumber = 0;
        int power = encodedNumber.Length - 1;
        int baseNumber = characters.Length;
        foreach (char c in encodedNumber)
        {
            decodedNumber += characters.IndexOf(c) * (long)Math.Pow(baseNumber, power);
            power--;
        }
        string decodedString = decodedNumber.ToString();
        string finalString = "";
        int length = decodedString.Length;
        
        for (int i = length - 1; i >= 0; i--) {
            finalString = decodedString[i] + finalString;
            if ((length - i) % 3 == 0 && i != 0) {
                finalString = "." + finalString;
            }
        }
        Debug.Log("Server IP: " + finalString);
        return finalString;
    }
}
