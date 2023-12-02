using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using System.Threading.Tasks;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class UM2_Server : MonoBehaviour
{
    public bool udpOnline;
    public bool tcpOnline;
    public bool httpOnline;
    int udpPort;
    int tcpPort;
    int httpPort;

    IPEndPoint remoteEndPoint;
    UdpClient udpClient;

    List<TcpClient> tcpClients = new List<TcpClient>();
    List<NetworkStream> tcpStreams = new List<NetworkStream>();

    public void StartServer(int _udpPort, int _tcpPort, int _httpPort)
    {
        udpPort = _udpPort;
        tcpPort = _tcpPort;
        httpPort = _httpPort;

        initTCP();
        initUDP();
    }

    void initUDP()
    {
        /*remoteEndPoint = new IPEndPoint(IPAddress.Any, udpPort);

        udpClient = new UdpClient();
        udpClient.Connect(SERVER_IP, udpPort);

        udpReciever();*/
    }

    void initTCP()
    {
        /*tcpClient = new TcpClient();
        tcpClient.Connect(SERVER_IP, tcpPort);
        tcpStream = tcpClient.GetStream();

        tcpReciever();*/
    }

    async void udpReciever()
    {
        while (true)
        {
            /*byte[] receiveBytes = new byte[0];
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
            }*/
        }
    }

    async void tcpReciever()
    {
        while (true)
        {
            /*byte[] tcpReceivedData = new byte[1024];
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
            }*/
        }
    }

    public void sendTCPMessage(string message, int clientID)
    {
        /*message += "|";
        sendBytesTCP += Encoding.UTF8.GetByteCount(message);
        byte[] tcpData = Encoding.ASCII.GetBytes(message);
        tcpStream.Write(tcpData, 0, tcpData.Length);*/
    }

    public void sendUDPMessage(string message, int clientID)
    {
        /*sendBytesUDP += Encoding.UTF8.GetByteCount(message);
        //load message
        byte[] udpData = Encoding.ASCII.GetBytes(message);

        //send message
        udpClient.Send(udpData, udpData.Length);*/
    }
}
