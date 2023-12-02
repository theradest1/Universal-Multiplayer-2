using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UM2_Server : MonoBehaviour
{
    public bool udpOnline;
    public bool tcpOnline;
    public bool httpOnline;
    int udpPort;
    int tcpPort;
    int httpPort;
    public (string, string) StartServer(int _udpPort, int _tcpPort, int _httpPort)
    {
        udpPort = _udpPort;
        tcpPort = _tcpPort;
        httpPort = _httpPort;

        string externalIP = "20348957";
        string internalIP = "32490509";
        return (externalIP, internalIP);
    }

    void initUDP()
    {
        remoteEndPoint = new IPEndPoint(IPAddress.Any, UDP_PORT);

        udpClient = new UdpClient();
        udpClient.Connect(SERVER_IP, UDP_PORT);

        udpReciever();
    }

    void initTCP()
    {
        tcpClient = new TcpClient();
        tcpClient.Connect(SERVER_IP, TCP_PORT);
        tcpStream = tcpClient.GetStream();

        tcpReciever();
    }

    async void udpReciever()
    {
        while (true)
        {
            byte[] receiveBytes = new byte[0];
            await Task.Run(() => receiveBytes = udpClient.Receive(ref remoteEndPoint));
            string message = Encoding.ASCII.GetString(receiveBytes);
            getBytesUDP += Encoding.UTF8.GetByteCount(message);

            try
            {
                processMessage("UDP", message);
            }
            catch
            {
                udpProcessErrors++;
                Debug.LogWarning("UDP process error: " + message);
            }
        }
    }

    async void tcpReciever()
    {
        while (true)
        {
            byte[] tcpReceivedData = new byte[1024];
            int bytesRead = 0; //this might cause problems, but I don't think so

            await Task.Run(() => bytesRead = tcpStream.Read(tcpReceivedData, 0, tcpReceivedData.Length));
            string message = Encoding.UTF8.GetString(tcpReceivedData, 0, bytesRead);

            getBytesTCP += Encoding.UTF8.GetByteCount(message);

            //Debug.Log("Got TCP Message: " + message);

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
                        tcpProcessErrors++;
                        Debug.LogWarning("TCP process error: " + finalMessage);
                    }
                }
            }
        }
    }

    public void sendTCPMessage(string message, clientID)
    {
        message += "|";
        sendBytesTCP += Encoding.UTF8.GetByteCount(message);
        byte[] tcpData = Encoding.ASCII.GetBytes(message);
        tcpStream.Write(tcpData, 0, tcpData.Length);
    }

    public void sendUDPMessage(string message, int clientID)
    {
        sendBytesUDP += Encoding.UTF8.GetByteCount(message);
        //load message
        byte[] udpData = Encoding.ASCII.GetBytes(message);

        //send message
        udpClient.Send(udpData, udpData.Length);
    }
}
