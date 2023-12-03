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

public class UM2_Client : MonoBehaviour
{
    public int clientTcpPort = 6000;
    public int clientUdpPort = 6001;
    public int clientHttpPort = 6002;

    public string serverIP = "127.0.0.1";
    public int serverUdpPort = 5000;
    public int serverTcpPort = 5001;
    public int serverHttpPort = 5002;

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
        if (protocol == "UDP")
        {
            Debug.Log("Got message through " + protocol + ": " + message);
        }
    }
}
