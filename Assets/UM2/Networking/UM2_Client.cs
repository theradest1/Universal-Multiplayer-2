using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using System.Threading.Tasks;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using System.Collections.Generic;
using Unity.VisualScripting;

public class UM2_Client : MonoBehaviour
{
    public static string serverIP = null;
    public static int serverUdpPort = 0;
    public static int serverTcpPort = 0;
    public static int serverHttpPort = 0;
    public static bool hostingServer = false;

    IPEndPoint serverEndpoint;
    UdpClient udpClient;
    bool connectedToUDP = false;

    TcpClient tcpClient;
    NetworkStream tcpStream;
    bool connectedToTCP = false;

    bool connectedToHTTP = false;


    float httpPingStartTime;
    float udpPingStartTime;
    float tcpPingStartTime;

    private void Start()
    {
        //get info from menu
        if (serverIP == null || serverUdpPort == 0 || serverUdpPort == 0 || serverTcpPort == 0)
        {
            Debug.LogWarning("Server info not set, pushing back to menu");
            SceneManager.LoadScene("Menu");
            return;
        }

        if (!hostingServer) //client gets started by server so it doesnt try to join before server is up
        {
            StartClient();
        }
    }

    public void StartClient()
    {
        initTCP();
        initUDP();
        initHTTP();

        InvokeRepeating("Ping", 0, 1f);
    }

    void Ping()
    {
        udpPingStartTime = Time.time;
        sendUDPMessage("ping");

        tcpPingStartTime = Time.time;
        sendTCPMessage("ping");

        httpPingStartTime = Time.time;
        sendHTTPMessage("ping");
    }

    void initUDP()
    {
        try
        {
            serverEndpoint = new IPEndPoint(IPAddress.Parse(serverIP), serverUdpPort);

            udpClient = new UdpClient();
            udpClient.Connect(serverEndpoint.Address, serverEndpoint.Port);

            udpReciever();
        }
        catch (Exception e)
        {
            Debug.Log("Couldnt start udp client: " + e);
        }
    }

    void initTCP()
    {
        /*tcpClient = new TcpClient();
        tcpClient.Connect(serverIP, tcpPort);
        tcpStream = tcpClient.GetStream();

        tcpReciever();*/
    }

    void initHTTP()
    {
        //do some stuff with http
    }

    async void udpReciever()
    {
        try
        {
            while (true)
            {
                UdpReceiveResult result = await udpClient.ReceiveAsync();
                string message = Encoding.ASCII.GetString(result.Buffer);

                processMessage(message, "UDP");
            }
        }
        catch (Exception e)
        {
            Debug.Log("UDP client exception: " + e);
        }
    }

    async void tcpReciever()
    {
        /*connectedToTCP = true;
        while (true)
        {
            byte[] tcpReceivedData = new byte[1024];
            int bytesRead = 0; //this might cause problems, but I don't think so

            await Task.Run(() => bytesRead = tcpStream.Read(tcpReceivedData, 0, tcpReceivedData.Length));
            string message = Encoding.UTF8.GetString(tcpReceivedData, 0, bytesRead);

            //getBytesTCP += Encoding.UTF8.GetByteCount(message);

            //loop through messages
            string[] messages = message.Split('|');
            foreach (string finalMessage in messages)
            {
                if (finalMessage != "") //to get rid of final message
                {
                    try
                    {
                        processMessage("TCP", finalMessage);
                    }
                    catch
                    {
                        //tcpProcessErrors++;
                        Debug.LogWarning("TCP process error: " + finalMessage);
                    }
                }
            }
        }*/
    }

    public void sendTCPMessage(string message)
    {
        /*if (connectedToTCP)
        {
            message += "|";
            //sendBytesTCP += Encoding.UTF8.GetByteCount(message);
            byte[] tcpData = Encoding.ASCII.GetBytes(message);
            tcpStream.Write(tcpData, 0, tcpData.Length);
        }*/
    }

    public void sendUDPMessage(string message)
    {
        //sendBytesUDP += Encoding.UTF8.GetByteCount(message);

        //load message
        byte[] udpData = Encoding.ASCII.GetBytes(message);

        //send message
        udpClient.Send(udpData, udpData.Length);
    }

    async void sendHTTPMessage(string message)
    {
        UnityWebRequest request = UnityWebRequest.Get("http://" + serverIP + ":" + serverHttpPort + "/" + message);

        // Send the request asynchronously
        //Debug.Log("Sent: " + "http://" + serverIP + ":" + serverHttpPort + "/" + message);
        var operation = request.SendWebRequest();

        // Wait for the request to complete
        while (!operation.isDone)
        {
            await Task.Yield();
        }

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error: " + request.error);
        }
        else
        {
            // Print the response data
            string response = request.downloadHandler.text;
            processMessage(response, "HTTP");
        }
    }

    void processMessage(string message, string protocol)
    {
        Debug.Log("Got message through " + protocol + ": " + message);
    }
}
