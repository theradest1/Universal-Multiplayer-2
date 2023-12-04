using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using System.Threading.Tasks;
using System.Threading;
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
    Thread tcpRecieveThread;

    bool connectedToHTTP = false;


    float udpPingStartTime;
    float udpPing;
    float tcpPingStartTime;
    float tcpPing;
    float httpPingStartTime;
    float httpPing;

    int sentBytes = 0;
    int receivedBytes = 0;
    int failedMessages = 0;

    public UM2_Server server;
    public Debugger debugger;

    private void Start()
    {
        UM2_Server.GetLocalIPAddress();
        UM2_Server.GetPublicIPAddress();

        //get info from menu
        if (serverIP == null || serverUdpPort == 0 || serverUdpPort == 0 || serverTcpPort == 0)
        {
            Debug.LogWarning("Server info not set, pushing back to menu");
            SceneManager.LoadScene("Menu");
            return;
        }

        //set up debug

        debugger.addSpace("Client:");
        debugger.setDebug("UDP Ping", "n/a");
        debugger.setDebug("TCP Ping", "n/a");
        debugger.setDebug("HTTP Ping", "n/a");
        debugger.setDebug("Bytes/Sec sent", "0");
        debugger.setDebug("Bytes/Sec recieved", "0");
        debugger.setDebug("Failed/Sec", "0");

        if (hostingServer)
        {
            debugger.addSpace();
            debugger.addSpace("Server:");
            debugger.setDebug("UDP status", "offline");
            debugger.setDebug("TCP status", "offline");
            debugger.setDebug("HTTP status", "offline");
            debugger.setDebug("Bytes/Sec sent ", "0");
            debugger.setDebug("Bytes/Sec recieved ", "0");
            debugger.setDebug("Failed/Sec ", "0");
        }

        if (!hostingServer) //client gets started by server so it doesnt try to join before server is up
        {
            StartClient();
        }
        else
        {
            server.StartServer();
        }

        InvokeRepeating("updateDebug", 1f, 1f);
    }

    void updateDebug()
    {
        debugger.setDebug("Bytes/Sec sent", sentBytes + "");
        debugger.setDebug("Bytes/Sec recieved", receivedBytes + "");
        debugger.setDebug("Failed/Sec", failedMessages + "");

        sentBytes = 0;
        receivedBytes = 0;
        failedMessages = 0;
    }

    public void StartClient()
    {
        initTCP();
        initUDP();
        initHTTP();

        InvokeRepeating("Ping", 0, .25f);
    }

    void Ping()
    {
        udpPingStartTime = Time.time;
        sendMessage("ping", "UDP");

        tcpPingStartTime = Time.time;
        sendMessage("ping", "TCP");

        httpPingStartTime = Time.time;
        sendMessage("ping", "HTTP");
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
            Debug.Log("Couldn't start udp client: " + e);
        }
    }

    void initTCP()
    {
        try
        {
            tcpClient = new TcpClient();
            tcpClient.Connect(IPAddress.Parse(serverIP), serverTcpPort);
            tcpStream = tcpClient.GetStream();

            connectedToTCP = true;

            tcpRecieveThread = new Thread(new ThreadStart(tcpReciever));
            tcpRecieveThread.Start();
        }
        catch (Exception e)
        {
            Debug.Log("Error connecting to server: " + e.Message);
        }
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
            failedMessages += 1;
        }
    }

    private void tcpReciever()
    {
        byte[] buffer = new byte[1024];
        while (connectedToTCP)
        {
            try
            {
                int bytesRead = tcpStream.Read(buffer, 0, buffer.Length);
                if (bytesRead > 0)
                {
                    string receivedData = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                    MainThreadDispatcher.Enqueue(() => processMessage(receivedData, "TCP"));
                }
            }
            catch (Exception e)
            {
                Debug.Log("Error receiving data: " + e.Message);
                failedMessages += 1;
            }
        }
    }

    public void sendTCPMessage(string message)
    {
        if (connectedToTCP)
        {
            try
            {
                byte[] data = Encoding.ASCII.GetBytes(message);
                tcpStream.Write(data, 0, data.Length);
            }
            catch (Exception e)
            {
                Debug.Log("Error sending data: " + e.Message);
                failedMessages += 1;
            }
        }
    }

    public void stopTCP()
    {
        connectedToTCP = false;

        if (tcpStream != null)
            tcpStream.Close();

        if (tcpClient != null && tcpClient.Connected)
            tcpClient.Close();
    }

    public void sendUDPMessage(string message)
    {
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
            failedMessages += 1;
        }
        else
        {
            // Print the response data
            string response = request.downloadHandler.text;
            processMessage(response, "HTTP");
        }
    }

    public void sendMessage(string message, string protocol)
    {
        sentBytes += System.Text.Encoding.UTF8.GetByteCount(message);

        if (protocol == "UDP")
        {
            sendUDPMessage(message);
        }
        else if (protocol == "TCP")
        {
            sendTCPMessage(message);
        }
        else if (protocol == "HTTP")
        {
            sendHTTPMessage(message);
        }
    }

    void processMessage(string message, string protocol)
    {
        receivedBytes += System.Text.Encoding.UTF8.GetByteCount(message);
        Debug.Log("Got message through " + protocol + ": " + message);

        if (message == "pong")
        {
            if (protocol == "UDP")
            {
                udpPing = Time.time - udpPingStartTime;
                debugger.setDebug("UDP Ping", (int)(udpPing * 1000) + "ms");
            }
            else if (protocol == "TCP")
            {
                tcpPing = Time.time - tcpPingStartTime;
                debugger.setDebug("TCP Ping", (int)(tcpPing * 1000) + "ms");
            }
            else if (protocol == "HTTP")
            {
                httpPing = Time.time - httpPingStartTime;
                debugger.setDebug("HTTP Ping", (int)(httpPing * 1000) + "ms");
            }
        }
        else
        {
            Debug.LogWarning("Got unknown message from " + protocol + ": " + message);
            failedMessages += 1;
        }
    }
}
