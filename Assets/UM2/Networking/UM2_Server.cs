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
using System.Runtime.InteropServices;
using Unity.VisualScripting;
using System.Xml.Schema;

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

public class ServerVariable
{
	public string name;
	public string value;
	public Type type;
    UM2_Server server;

	public ServerVariable(string setName, string setValue, string setType, UM2_Server setServer)
	{
		name = setName;
		value = setValue;

		type = Type.GetType(setType);
        server = setServer;
	}

    public void add(string addValue){
        if(type == typeof(int)){
            set(int.Parse(value) + int.Parse(addValue) + "");
        }
        else if(type == typeof(float)){
            set(float.Parse(value) + float.Parse(addValue) + "");
        }
        else if(type == typeof(string)){
            set(value + addValue);
        }
    }

    public void set(string newValue){
        value = newValue;
        Debug.Log("(Server) Set " + name + " to " + value);
        server.sendMessageToAll("syncVar~" + name + "~" + value, "TCP");
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
    int currentObjectID = 0;
    int currentVariableID = 0;


    List<Client> clients = new List<Client>();

    public bool debugMessages = false;

	//server-variables
	List<ServerVariable> serverVariables = new List<ServerVariable>();

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

        //close tcp
        if(tcpServer != null){
            tcpServer.Stop();
        }
        tcpOnline = false;
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
        InvokeRepeating("updateDebug", 1f, 1f);

        if (localIpAddress == null || publicIpAddress == null)
        {
            Debug.LogWarning("(Server) Ip addresses have not been found yet (restart if continued)");
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
            if(debugMessages){
                Debug.Log("(Server) HTTP Server started on port " + httpPort);
            }

            httpOnline = true;

            httpReciever();
        }
        catch (Exception e)
        {
            Debug.LogError("(Server) Failed to start HTTP: " + e.Message);
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
                        if(debugMessages){
                            Debug.Log("(Server) Got HTTP: " + message);
                        }
                        string responseMessage = processSplitMessages(message, "HTTP");
                        responseMessage += "|";

                        //send response - even if empty (because http needs a reponse)
                        byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseMessage);
                        response.ContentType = "text/html";
                        response.ContentLength64 = buffer.Length;
                        if(debugMessages){
                            Debug.Log("(Server) Sent HTTP: " + responseMessage);
                        }

                        sentBytes += System.Text.Encoding.UTF8.GetByteCount(responseMessage);
                        response.OutputStream.Write(buffer, 0, buffer.Length);
                        response.OutputStream.Close();
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError("(Server) Error handling request: " + e.Message);
                    failedMessages += 1;
                }
            }
        });
    }

    string processSplitMessages(string messages, string protocol){
        string[] splitMessages = messages.Split("|");
        string endString = "";
        foreach(string message in splitMessages){
            if(message != ""){
                endString += "|" + processMessage(message, protocol);
            }
        }

        return endString;
    }

    string processMessage(string message, string protocol)
    {
        receivedBytes += System.Text.Encoding.UTF8.GetByteCount(message);

        string clientIDString = message.Split("~")[0];
        string messageType = message.Split("~")[1];
        string messageContents = message.Substring(clientIDString.Length + messageType.Length + 2);
        string messageCommand;
        string[] messageParameters = message.Split("~");
        
        int clientID = int.Parse(clientIDString);

        string responseMessage = null;
        switch (messageType)
        {
            case "server":  //server messages (client -> server)
                messageCommand = message.Split("~")[2];
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
                    case "reserveObjectID":
                        responseMessage = "reservedObjectID~" + currentObjectID;
                        currentObjectID++;
                        break;
                    case "getQueue": //this is called by http clients to collect queued messages
                        responseMessage = "";
                        break;
                    case "newVar":
                        string varName = messageContents.Split("~")[1];
                        string varValue = messageContents.Split("~")[2];
                        string varType = messageContents.Split("~")[3];
                        serverVariables.Add(new ServerVariable(varName, varValue, varType, this));
                        sendMessageToAll("syncNewVar~" + varName + "~" + varValue + "~" + varType, "TCP");
                        Debug.Log("(Server) New variable: \nName: " + varName + "\nType: " + varType + "\nValue: " + varValue);
                        break;
                    case "setVar":
                        varName = messageContents.Split("~")[1];
                        varValue = messageContents.Split("~")[2];
                        Debug.Log("(Server) Setting var " + varName);
                        getServerVariable(varName).set(varValue);
                        break;
                    case "addToVar":
                        varName = messageContents.Split("~")[1];
                        varValue = messageContents.Split("~")[2];
                        Debug.Log("(Server) Adding to var " + varName);
                        getServerVariable(varName).add(varValue);
                        break;
                    default:
                        Debug.LogError("(Server) Unknown message from " + protocol + ": " + message);
                        break;
                }
                break;

            case "others":  //send message to all other clients
                sendMessageToAll(messageContents, protocol, clientID);
                break;
            case "all":     //send message to all clients
                sendMessageToAll(messageContents, protocol);
                break;
            case "direct":  //send message to specified other client
                int targetClientID = int.Parse(messageContents.Split("~")[0]);
                messageContents = messageContents.Substring(messageContents.Split("~")[0].Length + 1);
                foreach(Client client in clients){
                    if(client.clientID == targetClientID){
                        if(protocol == "UDP" && client.udpEndpoint != null){
                            SendUDPMessage(messageContents, client.udpEndpoint);
                        }
                        else if(client.tcpClient != null && client.networkStream != null){
                            sendTCPMessage(messageContents, client.networkStream);
                        }
                        else{
                            sendHTTPMessage(messageContents, clientID);
                        }
                        break;
                    }
                }
                Debug.LogError("(Server) Couldnt find client with ID " + targetClientID);
                break;
            default:
                Debug.LogError("(Server) Unknown message type: " + messageType + " (from " + message + ")");
                break;
        }

        //check if queued messages for http
        if(protocol == "HTTP" && clientID != -1){
            Client currentClient = getClientFromID(clientID);
            foreach(string queuedMessage in currentClient.messageQueue){
                responseMessage += "|" + queuedMessage;
            }
            //clear out queue
            currentClient.messageQueue = new List<string>();
        }
        return responseMessage;
    }

    public ServerVariable getServerVariable(string name){
        foreach(ServerVariable serverVariable in serverVariables){
            if(serverVariable.name == name){
                return serverVariable;
            }
        }
        Debug.LogError("(Server) Could not find server variable: " + name);
        return null;
    }

    public void sendMessageToAll(string message, string protocol, int excludedID = -1){
        foreach(Client client in clients){
            if(client.clientID != excludedID){
                if(protocol == "UDP" && client.udpEndpoint != null){
                    SendUDPMessage(message, client.udpEndpoint);
                }
                else if(client.tcpClient != null && client.networkStream != null){
                    sendTCPMessage(message, client.networkStream);
                }
                else{
                    sendHTTPMessage(message, client.clientID);
                }
            }
        }
    }

    void initUDP()
    {
        try
        {
            //create server
            udpServer = new UdpClient(udpPort);

            //make it call udpReciever when message
            udpServer.BeginReceive(udpReciever, null);
            if(debugMessages){
                Debug.Log("(Server) UDP Server started on port " + udpPort);
            }
            udpOnline = true;
            debugger.setDebug("UDP status", "online");
        }
        catch (Exception e)
        {
            Debug.LogError("(Server) Error starting UDP server: " + e.Message);
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
            if(debugMessages){
                Debug.Log("(Server) TCP Server started on port " + tcpPort);
            }

            while (tcpOnline)
            {
                TcpClient client = await tcpServer.AcceptTcpClientAsync();
                handleTcpClient(client);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Server) Error starting TCP server: " + e.Message);
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
                if(debugMessages){
                    Debug.Log("(Server) Got TCP: " + receivedMessage);
                }
                
                string responseMessage = processSplitMessages(receivedMessage, "TCP");

                //need to change how this works (right now it checks if a client exists every message)
                if(receivedMessage.Split("~")[0] != "-1"){
                    Client clientData = getClientFromID(int.Parse(receivedMessage.Split("~")[0]));
                    clientData.networkStream = stream;
                    clientData.tcpClient = client;
                }

                if (responseMessage != null && responseMessage != "||" && responseMessage != "|")
                {
                    sendTCPMessage(responseMessage, stream);
                }
            }

            // Close the client connection
            client.Close();
        }
        catch (Exception e)
        {
            Debug.LogError("(Server) Error handling client: " + e.Message);
            failedMessages += 1;
        }
    }

    async void sendTCPMessage(string message, NetworkStream stream)
    {   
        message += "|";
        byte[] sendData = Encoding.ASCII.GetBytes(message);
        if(debugMessages){
            Debug.Log("(Server) Sent TCP: " + message);
        }
        sentBytes += System.Text.Encoding.UTF8.GetByteCount(message);
        await stream.WriteAsync(sendData, 0, sendData.Length);
    }

    private void udpReciever(IAsyncResult result)
    {
        udpServer.BeginReceive(udpReciever, null);
        
        IPEndPoint clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
        byte[] receivedBytes = udpServer.EndReceive(result, ref clientEndPoint);
        string receivedData = Encoding.UTF8.GetString(receivedBytes);

        if(debugMessages){
            Debug.Log("(Server) Got UDP: " + receivedData);
        }

        string responseMessage = processSplitMessages(receivedData, "UDP");
        
        //need to change how this works (right now it checks if a client exists every message)
        if(receivedData.Split("~")[0] != "-1"){
            Client clientData = getClientFromID(int.Parse(receivedData.Split("~")[0]));
            clientData.udpEndpoint = clientEndPoint;
        }

        if (responseMessage != null && responseMessage != "|")
        {
            SendUDPMessage(responseMessage, clientEndPoint);
        }

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
            if(debugMessages){
                Debug.Log("(Server) Sent UDP: " + message);
            }
            sentBytes += System.Text.Encoding.UTF8.GetByteCount(message);
            udpServer.Send(data, data.Length, clientEndPoint);
        }
        catch (Exception e)
        {
            Debug.LogError("(Server) Error sending response: " + e.Message);
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
        Debug.LogError("(Server) No network adapters with an IPv4 address in the system! (when finding local ip)");
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
            Debug.LogError("(Server) Error getting public IP: " + request.error);
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

        Debug.LogError("(Server) Couldnt find client with ID " + clientID);
        return null;
    }
}
