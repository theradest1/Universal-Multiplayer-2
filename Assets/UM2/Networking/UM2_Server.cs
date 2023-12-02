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
        string url = "http://localhost:" + httpPort + "/";

        // Create HttpListener
        httpListener = new HttpListener();
        httpListener.Prefixes.Add(url);

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
                        HandleRequest(context);
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

    void HandleRequest(HttpListenerContext context)
    {
        // Handle incoming requests here
        // For example, you can get the request and send a response

        HttpListenerRequest request = context.Request;

        // Get the request method (GET, POST, etc.)
        string requestMethod = request.HttpMethod;

        // Get the request URL
        string requestUrl = request.RawUrl.Substring(1);

        Debug.Log("Received " + requestMethod + " request for: " + requestUrl);

        // Send a response
        HttpListenerResponse response = context.Response;
        string responseString = "pong";
        byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);

        // Set response headers and content
        response.ContentType = "text/html";
        response.ContentLength64 = buffer.Length;

        // Write the response
        response.OutputStream.Write(buffer, 0, buffer.Length);

        // Close the response
        response.Close();
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
