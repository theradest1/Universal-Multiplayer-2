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

    public UM2_Server server;
    public Debugger debugger;

    private void Start()
    {
        //get info from menu
        if (serverIP == null || serverUdpPort == 0 || serverUdpPort == 0 || serverTcpPort == 0)
        {
            Debug.LogWarning("Server info not set, pushing back to menu");
            SceneManager.LoadScene("Menu");
            return;
        }

        //set up debug
        debugger.addSpace();
        debugger.setDebug("UDP server status", "offline");
        debugger.setDebug("UDP ping", "n/a");
        debugger.addSpace();
        debugger.setDebug("TCP server status", "offline");
        debugger.setDebug("TCP ping", "n/a");
        debugger.addSpace();
        debugger.setDebug("HTTP server status", "offline");
        debugger.setDebug("HTTP ping", "n/a");

        if (!hostingServer) //client gets started by server so it doesnt try to join before server is up
        {
            StartClient();
        }
        else
        {
            server.StartServer();
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
            }
        }
    }

    public void sendTCPMessage(string message)
    {
        if (connectedToTCP)
        {
            try
            {
                //sendBytesTCP += Encoding.UTF8.GetByteCount(message);
                byte[] data = Encoding.ASCII.GetBytes(message);
                tcpStream.Write(data, 0, data.Length);
            }
            catch (Exception e)
            {
                Debug.Log("Error sending data: " + e.Message);
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

        if (message == "pong")
        {
            if (protocol == "UDP")
            {
                udpPing = Time.time - udpPingStartTime;
                debugger.setDebug("UDP ping", (int)(udpPing * 1000) + "ms");
            }
            else if (protocol == "TCP")
            {
                tcpPing = Time.time - tcpPingStartTime;
                debugger.setDebug("TCP ping", (int)(tcpPing * 1000) + "ms");
            }
            else if (protocol == "HTTP")
            {
                httpPing = Time.time - httpPingStartTime;
                debugger.setDebug("HTTP ping", (int)(httpPing * 1000) + "ms");
            }
        }
        else
        {
            Debug.LogWarning("Got unknown message from " + protocol + ": " + message);
        }
    }
}
