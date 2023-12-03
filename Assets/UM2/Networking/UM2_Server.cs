using System.Collections.Generic;
using System;
using System.Net;
using System.Net.Sockets;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class UM2_Server : MonoBehaviour
{
    int udpPort = 5000;
    int tcpPort = 5001;
    int httpPort = 5002;

    IPEndPoint remoteEndPoint;
    UdpClient udpClient;
    public bool udpOnline;

    List<TcpClient> tcpClients = new List<TcpClient>();
    List<NetworkStream> tcpStreams = new List<NetworkStream>();
    public bool tcpOnline;

    HttpListener httpListener;
    public bool httpOnline;

    public void StartServer()
    {
        initTCP();
        initUDP();
        initHTTP();
    }

    void initHTTP()
    {
        // Define the URL and port for the server
        string localHostURL = "http://127.0.0.1:" + httpPort + "/";
        string localURL = "http://192.168.0.16:" + httpPort + "/";
        string publicURL = "http://75.100.205.73:" + httpPort + "/";

        // Create HttpListener
        httpListener = new HttpListener();
        httpListener.Prefixes.Add(localHostURL);
        print(localHostURL);
        httpListener.Prefixes.Add(localURL);
        print(localURL);
        /*httpListener.Prefixes.Add(publicURL);
        print(publicURL);*/

        // Start listening for incoming requests
        try
        {
            httpListener.Start();
            httpOnline = true;

            // Start a new thread to handle incoming requests
            ThreadPool.QueueUserWorkItem((state) =>
            {
                while (httpOnline)
                {
                    try
                    {
                        // Wait for a request to come in
                        HttpListenerContext context = httpListener.GetContext();

                        // Handle the request in a separate function
                        processHTTPMessage(context);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError("Error handling request: " + e.Message);
                    }
                }
            });
            Debug.Log("HTTP Server started.");
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to start HTTP Server: " + e.Message);
            httpOnline = false;
        }
    }

    public void stopHttp()
    {
        httpOnline = false;
        httpListener.Stop();
        httpListener.Close();
    }

    void processHTTPMessage(HttpListenerContext context)
    {
        HttpListenerRequest request = context.Request;
        string message = request.RawUrl.Substring(1);
        processMessage(message);

        // Send a response
        HttpListenerResponse response = context.Response;
        string responseString = "pong";
        byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
        response.ContentType = "text/html";
        response.ContentLength64 = buffer.Length;
        response.OutputStream.Write(buffer, 0, buffer.Length);
        response.Close();
    }

    void processMessage(string message)
    {
        Debug.Log("Got message: " + message);
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
