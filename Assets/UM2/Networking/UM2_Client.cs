using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using System.Threading.Tasks;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class UM2_Client : MonoBehaviour
{
    public int tcpPort = 5000;
    public int udpPort = 5001;
    public int httpPort = 5002;
    public string serverIP = "127.0.0.1";

    IPEndPoint remoteEndPoint;
    UdpClient udpClient;
    bool connectedToUDP = false;

    TcpClient tcpClient;
    NetworkStream tcpStream;
    bool connectedToTCP = false;

    float httpPingStartTime;
    float udpPingStartTime;
    float tcpPingStartTime;


    public void StartClient(string _serverIP, int _udpPort, int _tcpPort, int _httpPort)
    {
        udpPort = _udpPort;
        tcpPort = _tcpPort;
        httpPort = _httpPort;
        serverIP = _serverIP;

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
        remoteEndPoint = new IPEndPoint(IPAddress.Any, udpPort);

        udpClient = new UdpClient();
        udpClient.Connect(serverIP, udpPort);

        udpReciever();
    }

    void initTCP()
    {
        tcpClient = new TcpClient();
        tcpClient.Connect(serverIP, tcpPort);
        tcpStream = tcpClient.GetStream();

        tcpReciever();
    }

    void initHTTP()
    {
        //do some stuff with http
    }

    async void udpReciever()
    {
        while (true)
        {
            byte[] receiveBytes = new byte[0];
            await Task.Run(() => receiveBytes = udpClient.Receive(ref remoteEndPoint));
            string message = Encoding.ASCII.GetString(receiveBytes);
            //getBytesUDP += Encoding.UTF8.GetByteCount(message);

            try
            {
                processMessage("UDP", message);
            }
            catch
            {
                //udpProcessErrors++;
                Debug.LogWarning("UDP process error: " + message);
            }
        }
    }

    async void tcpReciever()
    {
        connectedToTCP = true;
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
        }
    }

    public void sendTCPMessage(string message)
    {
        if (connectedToTCP)
        {
            message += "|";
            //sendBytesTCP += Encoding.UTF8.GetByteCount(message);
            byte[] tcpData = Encoding.ASCII.GetBytes(message);
            tcpStream.Write(tcpData, 0, tcpData.Length);
        }
    }

    public void sendUDPMessage(string message)
    {
        //sendBytesUDP += Encoding.UTF8.GetByteCount(message);

        //load message
        byte[] udpData = Encoding.ASCII.GetBytes(message);

        //send message
        udpClient.Send(udpData, udpData.Length);
    }

    public void sendHTTPMessage(string message)
    {
        //send an http message
    }

    void processMessage(string protocol, string message)
    {
        Debug.Log("Got message through " + protocol + ": " + message);
    }
}
