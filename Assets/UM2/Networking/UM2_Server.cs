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
using UnityEditor.Compilation;

public class Client{
    public IPEndPoint udpEndpoint;

    public TcpClient tcpClient;
    public NetworkStream networkStream;

    public int clientID;

    public List<string> messageQueue = new List<string>();

    public Client(int id){
        clientID = id;
    }
}

public class UM2_Server : MonoBehaviour
{
    int udpPort = 5000;
    int tcpPort = 5001;
    int httpPort = 5002;

    UdpClient udpServer;
    public bool udpOnline;

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

    int currentPlayerID = 0;


    List<Client> clients = new List<Client>();


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
        InvokeRepeating("updateDebug", 1f, 1f);

        if (localIpAddress == null || publicIpAddress == null)
        {
            Debug.LogWarning("Ip addresses have not been found yet (restart if continued)");
            Invoke("StartServer", 1f);
            return;
        }

        if (!UM2_Client.webGLBuild)
        {
            initTCP();
            initUDP();
        }
        initHTTP();

        client.StartClient();
    }

    void initHTTP()
    {
        // Create HttpListener
        httpListener = new HttpListener();

        //anything with port 5002
        //you do still need to port forward for external connections
        httpListener.Prefixes.Add("http://*:" + httpPort + "/");

        // Start listening for incoming requests
        try
        {
            httpListener.Start();
            debugger.setDebug("HTTP status", "online");
            //Debug.Log("HTTP Server started on port " + httpPort);

            httpOnline = true;

            httpReciever();
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to start HTTP Server: " + e.Message);
            httpOnline = false;
            debugger.setDebug("HTTP status", "offline");
        }
    }

    void sendHTTPMessage(string message, int clientID){
        getClientFromID(clientID).messageQueue.Add(message + "|");
    }

    void httpReciever()
    {
        // Start a new thread to handle incoming requests
        ThreadPool.QueueUserWorkItem((state) =>
        {
            while (httpOnline)
            {
                try
                {
                    // wait for message and get data
                    HttpListenerContext context = httpListener.GetContext();
                    HttpListenerResponse response = context.Response;
                    HttpListenerRequest request = context.Request;

                    //set headers (so webgl builds dont complain)
                    response.Headers.Add("Access-Control-Allow-Origin", "*");
                    if (request.HttpMethod == "OPTIONS")
                    {
                        response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
                        response.Headers.Add("Access-Control-Allow-Headers", "Accept, Content-Type");
                        response.Headers.Add("Access-Control-Allow-Credentials", "true");
                        response.StatusCode = (int)HttpStatusCode.OK;
                        response.Close();
                    }
                    else if (request.HttpMethod == "GET")
                    {
                        // process the message
                        string message = request.RawUrl.Substring(1);
                        string responseMessage = processMessage(message, "HTTP");
                        responseMessage += "|";

                        //send response
                        byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseMessage);
                        response.ContentType = "text/html";
                        response.ContentLength64 = buffer.Length;

                        Debug.Log("HTTP: " + responseMessage);

                        response.OutputStream.Write(buffer, 0, buffer.Length);
                        response.OutputStream.Close();
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError("Error handling request: " + e.Message);
                    failedMessages += 1;
                }
            }
        });
    }

    string processMessage(string message, string protocol)
    {
        receivedBytes += System.Text.Encoding.UTF8.GetByteCount(message);

        string clientIDString = message.Split("~")[0];
        string messageType = message.Split("~")[1];
        string messageContents = message.Substring(clientIDString.Length + messageType.Length + 2);
        string[] messageParameters = message.Split("~");
        
        int clientID = int.Parse(clientIDString);

        string responseMessage = null;
        switch (messageType)
        {
            case "server":  //server messages (client -> server)
                string messageCommand = message.Split("~")[2];
                messageContents = message.Substring(clientIDString.Length + messageType.Length + messageCommand.Length + 3 - 1);
                switch (messageCommand)
                {
                    case "ping":
                        responseMessage = "pong";
                        break;
                    case "join":
                        Client newClient = new Client(currentPlayerID);
                        clients.Add(newClient);

                        responseMessage = "setID~" + currentPlayerID + "";
                        currentPlayerID++;

                        break;
                    default:
                        Debug.LogError("Unknown message: " + message);
                        break;
                }
                break;
            case "others":  //send message to all other clients
                Debug.LogError("Not implimented: " + messageType + " (from " + message + ")");
                break;
            case "all":     //send message to all clients
                //Debug.Log("Sending: " + messageContents + "\n from: " + message);
                foreach(Client client in clients){
                    if(protocol == "UDP" && client.udpEndpoint != null){
                        SendUDPMessage(messageContents, client.udpEndpoint);
                    }
                    else if(client.tcpClient != null){
                        sendTCPMessage(messageContents, client.networkStream);
                    }
                    else{
                        sendHTTPMessage(messageContents, clientID);
                    }
                }
                //Debug.LogError("Not implimented: " + messageType + " (from " + message + ")");
                break;
            case "direct":  //send message to specified other client
                Debug.LogError("Not implimented: " + messageType + " (from " + message + ")");
                break;
            default:
                Debug.LogError("Unknown message type: " + messageType + " (from " + message + ")");
                break;
        }

        //check if queued messages for http
        if(protocol == "HTTP" && clientID != -1){
            foreach(string queuedMessage in getClientFromID(clientID).messageQueue){
                responseMessage += "|" + queuedMessage;
            }
        }

        if(responseMessage != null){
            sentBytes += System.Text.Encoding.UTF8.GetByteCount(responseMessage);
        }

        return responseMessage;
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

    void initTCP()
    {
        tcpReciever();
    }

    async void tcpReciever() //this doesnt actually recieve any messages, but sets up connections with other clients
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
                string responseMessage = processMessage(receivedMessage, "TCP");

                //need to change how this works (right now it checks if a client exists every message)
                if(receivedMessage.Split("~")[0] != "-1"){
                    Client clientData = getClientFromID(int.Parse(receivedMessage.Split("~")[0]));
                    clientData.networkStream = stream;
                    clientData.tcpClient = client;
                }

                if (responseMessage != null)
                {
                    sendTCPMessage(responseMessage, stream);
                }
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
        message += "|";
        byte[] sendData = Encoding.ASCII.GetBytes(message);

        Debug.Log("TCP: " + message);
        await stream.WriteAsync(sendData, 0, sendData.Length);
    }

    private void udpReciever(IAsyncResult result)
    {
        IPEndPoint clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
        byte[] receivedBytes = udpServer.EndReceive(result, ref clientEndPoint);
        string receivedData = Encoding.UTF8.GetString(receivedBytes);

        string responseMessage = processMessage(receivedData, "UDP");
        
        //need to change how this works (right now it checks if a client exists every message)
        if(receivedData.Split("~")[0] != "-1"){
            Client clientData = getClientFromID(int.Parse(receivedData.Split("~")[0]));
            clientData.udpEndpoint = clientEndPoint;
        }

        if (responseMessage != null)
        {
            SendUDPMessage(responseMessage, clientEndPoint);
        }

        udpServer.BeginReceive(udpReciever, null);
    }

    private void SendUDPMessage(string message, int clientID)
    {
        IPEndPoint udpEndpoint = getClientFromID(clientID).udpEndpoint;
        SendUDPMessage(message, udpEndpoint);
    }

    private void SendUDPMessage(string message, IPEndPoint clientEndPoint)
    {
        message += "|";
        try
        {
            //sendBytesUDP += Encoding.UTF8.GetByteCount(message);
            byte[] data = Encoding.UTF8.GetBytes(message);

            Debug.Log("UDP: " + message);
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

    public Client getClientFromID(int clientID){
        foreach(Client client in clients){
            if(client.clientID == clientID){
                return client;
            }
        }

        Debug.LogError("Couldnt find client with ID " + clientID);
        return null;
    }

    private void OnDestroy()
    {
        //stop udp
        if (udpServer != null)
        {
            udpServer.Close();
        }
        udpOnline = false;
        //Debug.Log("UDP Server has been stopped");

        //stop http
        //Debug.Log("HTTP has been stopped");
        httpOnline = false;
        if (httpListener != null)
        {
            httpListener.Stop();
            httpListener.Close();
        }
    }
}
