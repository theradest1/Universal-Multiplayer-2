using System.Collections.Generic;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;

public class Client{
    public IPEndPoint udpEndpoint;

    public TcpClient tcpClient;
    public NetworkStream networkStream;

    public int clientID;

    public List<string> messageQueue = new List<string>();
    
    public float lastMessageTime;

    public Client(int id){
        clientID = id;
    }

    public void resettimeoutTimer(){
        lastMessageTime = Time.time;
    }
}

public class NetworkVariable_Server
{
	public string name;
	public string value;
    public int id;
    public int linkedID;
	public Type type;
    UM2_Server server;

	public NetworkVariable_Server(string name, int id, object value, Type type, int linkedID)
	{
		this.name = name;
		this.value = value + "";
        this.id = id;
		this.type = type;
        this.linkedID = linkedID;

        server = UM2_Server.instance;

        server.sendMessageToAll("syncNewVar~" + name + "~" + id + "~" + value + "~" + type + "~" + linkedID, "TCP");
	}

    public void add(object addValue){
        if(type == typeof(int)){
            set(int.Parse(value) + int.Parse(addValue + ""));
        }
        else if(type == typeof(float)){
            set(float.Parse(value) + float.Parse(addValue + ""));
        }
        else if(type == typeof(string)){
            set(value + addValue);
        }
    }

    public void set(object newValue){
        value = newValue + "";
        //Debug.Log("(Server) Set " + name + " to " + value);
        server.sendMessageToAll("syncVar~" + name + "~" + value + "~" + linkedID, "TCP");
    }
}

public class UM2_Server : MonoBehaviour
{
    public static string localIpAddress;
    public static string publicIpAddress;

    int udpPort = 5000;
    int tcpPort = 5001;
    int httpPort = 5002;

    UM2_Client client;
    public static UM2_Server instance;
    float currentTime;


    [Header("Settings:")]
    [Tooltip("How long (in seconds) it takes for a client to be timed out")]
    public float timeoutTime = 5; //seconds
    //public bool forceVersion = false;
    //public string version = "V0.0.0";

    [Header("Debug:")]
    public bool udpOnline;
    UdpClient udpServer;

    TcpListener tcpServer;
    public bool tcpOnline;

    HttpListener httpListener;
    public bool httpOnline;



    public int sentBytesUDP = 0;
    public int gotBytesUDP = 0;

    public int sentBytesTCP = 0;
    public int gotBytesTCP = 0;

    public int sentBytesHTTP = 0;
    public int gotBytesHTTP = 0;

    public int failedMessages = 0;


    [SerializeField] int currentPlayerID = 0; //each client has a seperate player ID
    [SerializeField] int currentObjectID = 0; //each object has a seperate object ID
    [SerializeField] int currentNetworkVarID = 0; //each network variable has a seperate ID (includes both object based and global network variables)


    [Header("Console debug settings:")]
    public bool debugUDPMessages = false;
    public bool debugTCPMessages = false;
    public bool debugHTTPMessages = false;
    public bool debugBasicMessages = false;


    List<Client> clients = new List<Client>();
	List<NetworkVariable_Server> networkVariables = new List<NetworkVariable_Server>();

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        client = UM2_Client.instance;
    }

	private void OnDestroy()
    {
        //stop udp
        if (udpServer != null)
        {
            udpServer.Close();
        }
        udpOnline = false;

        //stop http
        httpOnline = false;
        if (httpListener != null)
        {
            httpListener.Stop();
            httpListener.Close();
        }

        //stop tcp
        if(tcpServer != null){
            tcpServer.Stop();
        }
        tcpOnline = false;
    }

    private void Update() {
        //this is disgusting, but for some reason I cant get the time while in the http thread
        currentTime = Time.time; 
    }

    public void StartServer()
    {
        InvokeRepeating("checkTimeoutTimers", 1f, 1f);

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

    void checkTimeoutTimers(){
        List<Client> clientsToDisconnect = new List<Client>();
        foreach(Client client in clients){
            if(timeoutTime <= Time.time - client.lastMessageTime){
                sendMessageToAll("clientDisconnected~" + client.clientID, "TCP", client.clientID);
                clientsToDisconnect.Add(client);
            }
        }

        foreach(Client client in clientsToDisconnect){
            clients.Remove(client);
        }
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
            if(debugBasicMessages){
                Debug.Log("(Server) HTTP Server started on port " + httpPort);
            }

            httpOnline = true;

            httpReciever();
        }
        catch (Exception e)
        {
            Debug.LogError("(Server) Failed to start HTTP: " + e);
            httpOnline = false;
        }
    }

    void sendHTTPMessage(string message, int clientID){
        getClient(clientID).messageQueue.Add(message + "|");
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
                        string responseMessage = processSplitMessages(message, "HTTP");
                        responseMessage += "|";

                        //send response - even if empty (because http needs a reponse)
                        byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseMessage);
                        response.ContentType = "text/html";
                        response.ContentLength64 = buffer.Length;
                        if(debugHTTPMessages){
                            Debug.Log("(Server) Sent HTTP: " + responseMessage);
                        }

                        sentBytesHTTP += System.Text.Encoding.UTF8.GetByteCount(responseMessage);
                        response.OutputStream.Write(buffer, 0, buffer.Length);
                        response.OutputStream.Close();
                    }
                }
                catch (Exception e)
                {
                    if(e.Message != "Listener closed"){
                        Debug.LogError("(Server) Error handling request: " + e);
                        failedMessages += 1;
                    }
                    else if(debugBasicMessages){
                        Debug.Log("(Server) HTTP server closed");
                    }
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
        if (protocol == "UDP")
        {
            gotBytesUDP += System.Text.Encoding.UTF8.GetByteCount(message);
            if(debugUDPMessages){
                Debug.Log("(Server) Got UDP: " + message);
            }
        }
        else if (protocol == "TCP")
        {
            gotBytesTCP += System.Text.Encoding.UTF8.GetByteCount(message);
            if(debugTCPMessages){
                Debug.Log("(Server) Got TCP: " + message);
            }
        }
        else if (protocol == "HTTP")
        {
            gotBytesHTTP += System.Text.Encoding.UTF8.GetByteCount(message);
            if(debugHTTPMessages){
                Debug.Log("(Server) Got HTTP: " + message);
            }
        }

        string clientIDString = message.Split("~")[0];
        string messageType = message.Split("~")[1];
        string messageContents = message.Substring(clientIDString.Length + messageType.Length + 2);
        string messageCommand;
        //string[] messageParameters = message.Split("~");
        
        int clientID = int.Parse(clientIDString);

        if(clientID != -1){
            getClient(clientID).lastMessageTime = currentTime;
        }

        string responseMessage = null;
        switch (messageType)
        {
            case "server":  //server messages (client -> server)
                messageCommand = message.Split("~")[2];
                messageContents = message.Substring(clientIDString.Length + messageType.Length + messageCommand.Length + 3 - 1);
                switch (messageCommand)
                {
                    case "saveProtocol":
                        responseMessage = "recordedProtocol~" + protocol;
                        break;
                    case "ping":
                        responseMessage = "pong~" + protocol;
                        break;
                    case "join":
                        Client newClient = new Client(currentPlayerID);
                        clients.Add(newClient);

                        responseMessage = "setID~" + currentPlayerID + "";
                        sendMessageToAll("clientJoined~" + currentPlayerID, "TCP", currentPlayerID);
                        currentPlayerID++;

                        break;
                    case "reserveObjectID":
                        responseMessage = "reservedObjectID~" + currentObjectID;
                        currentObjectID++;
                        break;
                    case "reserveVariableID":
                        responseMessage = "reservedVariableID~" + currentNetworkVarID;
                        currentNetworkVarID++;
                        break;
                    case "getQueue": //this is called by http clients to collect queued messages
                        responseMessage = "";
                        break;
                    case "newVar":
                        string varName = messageContents.Split("~")[1];
                        int varID = int.Parse(messageContents.Split("~")[2]);
                        string varValue = messageContents.Split("~")[3];
                        string varType = messageContents.Split("~")[4];
                        int varLinkedID = int.Parse(messageContents.Split("~")[5]);

                        networkVariables.Add(new NetworkVariable_Server(varName, varID, varValue, Type.GetType(varType), varLinkedID));
                        //Debug.Log("(Server) New variable: \nName: " + varName + "\nType: " + varType + "\nValue: " + varValue);
                        break;
                    case "setVarValue":
                        varID = int.Parse(messageContents.Split("~")[1]);
                        varValue = messageContents.Split("~")[2];
                        varLinkedID = int.Parse(messageContents.Split("~")[3]);
                        //Debug.Log("(Server) Setting var " + varName);
                        getNetworkVariable(varID, varLinkedID).set(varValue);
                        break;
                    case "addToVarValue":
                        varID = int.Parse(messageContents.Split("~")[1]);
                        varValue = messageContents.Split("~")[2];
                        varLinkedID = int.Parse(messageContents.Split("~")[3]);
                        //Debug.Log("(Server) Adding to var " + varName);
                        getNetworkVariable(varID, varLinkedID).add(varValue);
                        break;
                    case "giveAllVariables":
                        //Debug.Log("giving all variables");
                        foreach(NetworkVariable_Server networkVariable in networkVariables){
                            varName = networkVariable.name;
                            varID = networkVariable.id;
                            varValue = (string)networkVariable.value;
                            varType = networkVariable.type + "";
                            varLinkedID = networkVariable.linkedID;
                            sendMessageToClient("syncNewVar~" + varName + "~" + varID + "~" + varValue + "~" + varType + "~" + varLinkedID, protocol, clientID);
                        }
                        break;
                    case "disconnect":
                        Client disconnectedClient = getClient(clientID);
                        sendMessageToAll("clientDisconnected~" + disconnectedClient.clientID, "TCP", disconnectedClient.clientID);
                        clients.Remove(disconnectedClient);
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
                Client client = getClient(targetClientID);
                
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
            default:
                Debug.LogError("(Server) Unknown message type: " + messageType + " (from " + message + ")");
                break;
        }

        //check if queued messages for http
        if(protocol == "HTTP" && clientID != -1){
            Client currentClient = getClient(clientID);
            foreach(string queuedMessage in currentClient.messageQueue){
                responseMessage += "|" + queuedMessage;
            }
            //clear out queue
            currentClient.messageQueue = new List<string>();
        }
        return responseMessage;
    }

    public NetworkVariable_Server getNetworkVariable(int id, int linkedID){
        foreach(NetworkVariable_Server networkVariable in networkVariables){
            if(networkVariable.id == id && networkVariable.linkedID == linkedID){
                return networkVariable;
            }
        }
        Debug.LogError("(Server) Could not find network variable with id " + id + " and synced id " + linkedID);
        return null;
    }

    public Client getClient(int clientID){
        foreach(Client client in clients){
            if(client.clientID == clientID){
                return client;
            }
        }
        Debug.LogError("(Server) Couldnt find client with ID " + clientID);
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

    public void sendMessageToClient(string message, string protocol, int clientID){
        Client client = getClient(clientID);
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

    void initUDP()
    {
        try
        {
            //create server
            udpServer = new UdpClient(udpPort);

            //make it call udpReciever when message
            udpServer.BeginReceive(udpReciever, null);
            if(debugBasicMessages){
                Debug.Log("(Server) UDP Server started on port " + udpPort);
            }
            udpOnline = true;
        }
        catch (Exception e)
        {
            Debug.LogError("(Server) Error starting UDP server: " + e);
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
            if(debugBasicMessages){
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
            if (e.GetType() != typeof(ObjectDisposedException)){ //if the server gets closed
                Debug.LogError("Server) Error starting TCP server: " + e);
            }
            else if(debugBasicMessages){
                Debug.Log("(Server) Closed TCP server");
            }
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
                
                string responseMessage = processSplitMessages(receivedMessage, "TCP");

                if (responseMessage != null && responseMessage != "||" && responseMessage != "|")
                {
                    if(responseMessage.Contains("recordedProtocol~TCP")){
                        Client clientData = getClient(int.Parse(receivedMessage.Split("~")[0]));
                        clientData.networkStream = stream;
                        clientData.tcpClient = client;

                        //Debug.Log("(Server) Recorded client's TCP stream: " + clientData.clientID + "\n" + stream + "\n" + client);
                    }
                    sendTCPMessage(responseMessage, stream);
                }
            }

            // Close the client connection
            client.Close();
        }
        catch (Exception e)
        {
            Debug.LogError("(Server) Error handling client: " + e);
            failedMessages += 1;
        }
    }

    async void sendTCPMessage(string message, NetworkStream stream)
    {   
        try{
            message += "|";
            byte[] sendData = Encoding.ASCII.GetBytes(message);
            if(debugTCPMessages){
                Debug.Log("(Server) Sent TCP: " + message);
            }
            sentBytesTCP += System.Text.Encoding.UTF8.GetByteCount(message);
            await stream.WriteAsync(sendData, 0, sendData.Length);
        }
        catch(Exception e){
            if(e.GetType() != typeof(ObjectDisposedException)){
                Debug.LogError(e);
            }
        }
    }

    private void udpReciever(IAsyncResult result)
    {
        udpServer.BeginReceive(udpReciever, null);
        
        IPEndPoint clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
        byte[] receivedBytes = udpServer.EndReceive(result, ref clientEndPoint);
        string receivedData = Encoding.UTF8.GetString(receivedBytes);

        string responseMessage = processSplitMessages(receivedData, "UDP");

        if (responseMessage != null && responseMessage != "|")
        {
            if(responseMessage.Contains("recordedProtocol~UDP")){
                Client clientData = getClient(int.Parse(receivedData.Split("~")[0]));
                clientData.udpEndpoint = clientEndPoint;
            }
            SendUDPMessage(responseMessage, clientEndPoint);
        }

    }

    private void SendUDPMessage(string message, IPEndPoint clientEndPoint)
    {
        message += "|";
        try
        {
            //sendBytesUDP += Encoding.UTF8.GetByteCount(message);
            byte[] data = Encoding.UTF8.GetBytes(message);
            if(debugUDPMessages){
                Debug.Log("(Server) Sent UDP: " + message);
            }
            sentBytesUDP += System.Text.Encoding.UTF8.GetByteCount(message);
            udpServer.Send(data, data.Length, clientEndPoint);
        }
        catch (Exception e)
        {
            Debug.LogError("(Server) Error sending response: " + e);
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
}
