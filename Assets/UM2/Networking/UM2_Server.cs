using System.Collections.Generic;
using System;
using System.Net;
using System.Net.Sockets;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using UnityEngine.SceneManagement;

public class UM2_Server : MonoBehaviour
{
    int udpPort = 5000;
    int tcpPort = 5001;
    int httpPort = 5002;

    List<IPEndPoint> udpClients = new List<IPEndPoint>();
    UdpClient udpServer;
    public bool udpOnline;

    List<TcpClient> tcpClients = new List<TcpClient>();
    List<NetworkStream> tcpStreams = new List<NetworkStream>();
    public bool tcpOnline;

    HttpListener httpListener;
    public bool httpOnline;

    string localIpAddress;
    string publicIpAddress;

    private void Start()
    {
        localIpAddress = GetLocalIPAddress();
        GetPublicIPAddress();
    }

    public void StartServer()
    {
        if (localIpAddress == null || publicIpAddress == null)
        {
            Debug.LogWarning("Ip addresses have not been found yet - try again");
            GetPublicIPAddress();
            return;
        }
        print("local: " + localIpAddress);
        print("public: " + publicIpAddress);

        initTCP();
        initUDP();
        initHTTP();
    }

    void initHTTP()
    {
        // Create HttpListener
        httpListener = new HttpListener();

        //anything with port 5002
        //you do still need to port forward for external
        httpListener.Prefixes.Add("http://*:5002/");

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
            Debug.Log("HTTP Server started on port " + httpPort);
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
        processMessage(message, "HTTP");

        // Send a response
        HttpListenerResponse response = context.Response;
        string responseString = "pong";
        byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
        response.ContentType = "text/html";
        response.ContentLength64 = buffer.Length;
        response.OutputStream.Write(buffer, 0, buffer.Length);
        response.Close();
    }

    void processMessage(string message, string protocol)
    {
        //Debug.Log("Got message: " + message);
    }

    void initUDP()
    {
        try
        {
            //create server
            udpServer = new UdpClient(udpPort);

            //make it call udpReciever when message
            udpServer.BeginReceive(udpReciever, null);

            Debug.Log("UDP Server started on port " + udpPort);
            udpOnline = true;
        }
        catch (Exception e)
        {
            Debug.LogError("Error starting UDP server: " + e.Message);
        }
    }

    void stopUDP()
    {
        udpServer.Close();
        udpOnline = false;
        Debug.Log("UDP Server has been stopped");
    }

    void initTCP()
    {
        /*tcpClient = new TcpClient();
        tcpClient.Connect(SERVER_IP, tcpPort);
        tcpStream = tcpClient.GetStream();

        tcpReciever();*/
    }

    private void udpReciever(IAsyncResult result)
    {
        IPEndPoint clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
        byte[] receivedBytes = udpServer.EndReceive(result, ref clientEndPoint);
        string receivedData = Encoding.UTF8.GetString(receivedBytes);

        SendUDPMessage("pong", clientEndPoint);

        udpServer.BeginReceive(udpReciever, null);
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

    private void SendUDPMessage(string message, int clientID)
    {
        IPEndPoint clientEndPoint = udpClients[clientID];
        SendUDPMessage(message, clientEndPoint);
    }

    private void SendUDPMessage(string message, IPEndPoint clientEndPoint)
    {
        try
        {
            //sendBytesUDP += Encoding.UTF8.GetByteCount(message);
            byte[] data = Encoding.UTF8.GetBytes(message);
            udpServer.Send(data, data.Length, clientEndPoint);
        }
        catch (Exception e)
        {
            Debug.LogError("Error sending response: " + e.Message);
        }
    }

    public string GetLocalIPAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }
        throw new System.Exception("No network adapters with an IPv4 address in the system!");
    }

    public async void GetPublicIPAddress()
    {
        //this is kind of disgusting but there isnt a different way
        UnityWebRequest request = UnityWebRequest.Get("http://checkip.dyndns.org");

        // send
        var operation = request.SendWebRequest();
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
            // get data
            string response = request.downloadHandler.text;

            //cut off unwanted things
            response = response.Substring(response.IndexOf(":") + 2);
            response = response.Substring(0, response.IndexOf("<"));

            publicIpAddress = response;
        }
    }
}
