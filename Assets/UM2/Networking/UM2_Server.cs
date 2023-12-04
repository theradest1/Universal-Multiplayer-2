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
    TcpListener tcpServer;
    public bool tcpOnline;

    HttpListener httpListener;
    public bool httpOnline;

    public static string localIpAddress;
    public static string publicIpAddress;


    int sentBytes = 0;
    int receivedBytes = 0;
    int failedMessages = 0;

    public UM2_Client client;

    public Debugger debugger;

    private void Start()
    {
        GetLocalIPAddress();
        GetPublicIPAddress();

        InvokeRepeating("updateDebug", 1f, 1f);
    }

    void updateDebug()
    {
        debugger.setDebug("Bytes/Sec sent ", sentBytes + "");
        debugger.setDebug("Bytes/Sec recieved ", receivedBytes + "");
        debugger.setDebug("Failed/Sec ", failedMessages + "");

        sentBytes = 0;
        receivedBytes = 0;
        failedMessages = 0;
    }

    public void StartServer()
    {
        if (localIpAddress == null || publicIpAddress == null)
        {
            Debug.LogWarning("Ip addresses have not been found yet - will be tried again soon");
            GetLocalIPAddress();
            GetPublicIPAddress();
            Invoke("StartServer", 1f);
            return;
        }
        print("local: " + localIpAddress);
        print("public: " + publicIpAddress);

        initTCP();
        initUDP();
        initHTTP();

        client.StartClient();
    }

    void initHTTP()
    {
        // Create HttpListener
        httpListener = new HttpListener();

        //anything with port 5002
        //you do still need to port forward for external
        httpListener.Prefixes.Add("http://*:" + httpPort + "/");

        // Start listening for incoming requests
        try
        {
            httpListener.Start();
            debugger.setDebug("HTTP status", "online");
            //Debug.Log("HTTP Server started on port " + httpPort);

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
                        failedMessages += 1;
                    }
                }
            });
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to start HTTP Server: " + e.Message);
            httpOnline = false;
            debugger.setDebug("HTTP status", "offline");
        }
    }

    public void stopHttp()
    {
        debugger.setDebug("HTTP status", "offline");
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
        sentBytes += System.Text.Encoding.UTF8.GetByteCount(responseString);
        byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
        response.ContentType = "text/html";
        response.ContentLength64 = buffer.Length;
        response.OutputStream.Write(buffer, 0, buffer.Length);
        response.Close();
    }

    void processMessage(string message, string protocol)
    {
        receivedBytes += System.Text.Encoding.UTF8.GetByteCount(message);
        //Debug.Log("Got message from " + protocol + ": " + message);
    }

    void initUDP()
    {
        try
        {
            //create server
            udpServer = new UdpClient(udpPort);

            //make it call udpReciever when message
            udpServer.BeginReceive(udpReciever, null);

            //Debug.Log("UDP Server started on port " + udpPort);
            udpOnline = true;
            debugger.setDebug("UDP status", "online");
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
        debugger.setDebug("UDP status", "offline");
        Debug.Log("UDP Server has been stopped");
    }

    void initTCP()
    {
        tcpReciever();
    }

    async void tcpReciever()
    {
        try
        {
            tcpServer = new TcpListener(IPAddress.Any, tcpPort);
            tcpServer.Start();
            tcpOnline = true;
            debugger.setDebug("TCP status", "online");

            //Debug.Log("TCP Server started on port " + tcpPort);

            while (tcpOnline)
            {
                TcpClient client = await tcpServer.AcceptTcpClientAsync();
                handleTcpClient(client);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error starting TCP server: " + e.Message);
        }
    }

    private async void handleTcpClient(TcpClient client)
    {
        try
        {
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[1024];
            int bytesRead;

            while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) != 0)
            {
                string receivedMessage = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                processMessage(receivedMessage, "TCP");

                sendTCPMessage("pong", stream);
            }

            // Close the client connection
            client.Close();
        }
        catch (Exception e)
        {
            Debug.LogError("Error handling client: " + e.Message);
            failedMessages += 1;
        }
    }

    async void sendTCPMessage(string message, NetworkStream stream)
    {
        sentBytes += System.Text.Encoding.UTF8.GetByteCount(message);
        byte[] sendData = Encoding.ASCII.GetBytes(message);
        await stream.WriteAsync(sendData, 0, sendData.Length);
    }

    private void udpReciever(IAsyncResult result)
    {
        IPEndPoint clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
        byte[] receivedBytes = udpServer.EndReceive(result, ref clientEndPoint);
        string receivedData = Encoding.UTF8.GetString(receivedBytes);

        SendUDPMessage("pong", clientEndPoint);

        udpServer.BeginReceive(udpReciever, null);
    }

    private void SendUDPMessage(string message, int clientID)
    {
        IPEndPoint clientEndPoint = udpClients[clientID];
        SendUDPMessage(message, clientEndPoint);
    }

    private void SendUDPMessage(string message, IPEndPoint clientEndPoint)
    {
        sentBytes += System.Text.Encoding.UTF8.GetByteCount(message);
        try
        {
            //sendBytesUDP += Encoding.UTF8.GetByteCount(message);
            byte[] data = Encoding.UTF8.GetBytes(message);
            udpServer.Send(data, data.Length, clientEndPoint);
        }
        catch (Exception e)
        {
            Debug.LogError("Error sending response: " + e.Message);
            failedMessages += 1;
        }
    }

    public static void GetLocalIPAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                UM2_Server.localIpAddress = ip.ToString();
                return;
            }
        }
        Debug.LogError("No network adapters with an IPv4 address in the system! (when finding local ip)");
    }

    public static async void GetPublicIPAddress()
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

            UM2_Server.publicIpAddress = response;
        }
    }
}
