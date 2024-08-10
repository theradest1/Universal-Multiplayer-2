using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using System.Threading.Tasks;
using System.Threading;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using System.Collections.Generic;
using UnityEngine.UIElements;
using Unity.VisualScripting;

public class UM2_Client : MonoBehaviourUM2
{
    #region Variables
    public static string serverIP = null;
    public static int serverUdpPort = 5000;
    public static int serverTcpPort = 5001;
    public static int serverHttpPort = 5002;
    public static bool hostingServer = false;
    public static bool webGLBuild = false;
    public static int clientID = -1;
    
    public static UM2_Client instance;

    List<String> messageQueue = new List<string>();
    //string messageQueue = "";


    [Header("Settings:")]
    [Tooltip("How often http clears the message queue (higher means less latency, but also more messages)")]
    public float httpUpdateTPS = 10;


    [Header("Console debug settings:")]
    public bool debugUDPMessages = false;
    public bool debugTCPMessages = false;
    public bool debugHTTPMessages = false;
    public bool debugBasicMessages = false;


    //udp
    IPEndPoint serverEndpoint;
    [Header("Debug variables:")]
    public bool udpRecorded = false; 
    UdpClient udpClient;
    bool connectedToUDP = false;
    bool UDPOnline = false;

    float udpPingStartTime;
    public float udpPing;
    public int sentBytesUDP;
    public int gotBytesUDP;

    //tcp
    Thread tcpRecieveThread;
    public bool tcpRecorded = false; //this is if the server has said that it recorded the client's TCP stream
    bool TCPOnline = false; //this is if the reciever is on
    TcpClient tcpClient;
    NetworkStream tcpStream;
    bool connectedToTCP = false; //this is if the server can be pinged
    float tcpPingStartTime;
    public float tcpPing;
    public int sentBytesTCP;
    public int gotBytesTCP;

    //http
    bool connectedToHTTP = false;
    float httpPingStartTime;
    public float httpPing;
    public int sentBytesHTTP;
    public int gotBytesHTTP;

    public int failedMessages = 0;


    public static bool connectedToServer = true;
    #endregion

    #region Events
    private void OnDestroy()
    {
        connectedToServer = false;

        //close tcp
        TCPOnline = false;
        if (tcpStream != null)
            tcpStream.Close();

        if (tcpClient != null && tcpClient.Connected)
            tcpClient.Close();
    }

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        connectedToServer = true;
        //get info from menu
        if (serverIP == null || serverUdpPort == 0 || serverUdpPort == 0 || serverTcpPort == 0)
        {
            Debug.LogError("Not all info has been set for client. Look at the startup info in the docs for help: \nServer IP: " + serverIP + "\nServer UDP Port: " + serverUdpPort + "\nServer TCP Port: " + serverTcpPort + "\nServer HTTP Port: " + serverHttpPort);
            #if UNITY_EDITOR
                SceneManager.LoadScene("Menu");
            #endif
            return;
        }

        if (!hostingServer) //client gets started by server so it doesnt try to join before server is up
        {
            StartClient();
        }
        else
        {
            UM2_Server.instance.StartServer();
        }
        InvokeRepeating("clearHTTPQueue", 1f, 1f/httpUpdateTPS);
    }

    public void StartClient()
    {
        if (!webGLBuild)
        {
            initTCP();
            initUDP();
        }
        initHTTP();

        InvokeRepeating("Ping", 0, 1f);

        sendMessage("server~join", true, true);
    }
    #endregion

    #region Basic message sending

    public void sendMessage(string message, bool reliableProtocol = true, bool sendWithoutID = false){ //this just finds what protocol you want to use and has some protections
        
        if(!reliableProtocol && connectedToUDP){
            sendMessage(message, "UDP", sendWithoutID);
            return;
        }
        else if(connectedToTCP){
            sendMessage(message, "TCP", sendWithoutID);
            return;
        }
        else if(connectedToHTTP){
            sendMessage(message, "HTTP", sendWithoutID);
            return;
        }
        
        if(reliableProtocol){
            if(debugBasicMessages){
                Debug.Log("Added message to queue: " + message);
            }
            messageQueue.Add(message);
        }
    }

    public async void sendMessage(string message, string protocol, bool sendWithoutID = false)
    {
        //check if wrong protocol is used
        if ((protocol == "UDP" || protocol == "TCP") && webGLBuild)
        {
            Debug.LogError(protocol + " cannot be used in a webGL build. Message trying to send was " + message);
            failedMessages += 1;
            return;
        }

        //sign message if ID is available
        if(!sendWithoutID){
            if(clientID == -1){
                while(clientID == -1){
                    await Task.Delay(100);
                    Debug.Log("Wating for ID to send message: " + message + ". Current ID: " + clientID);
                    if(!connectedToServer){
                        Debug.LogWarning("Didn't send message " + message);
                        return;
                    }
                }
            }
        }

        //prepping dividers
        message = clientID + "~" + message;

        //UDP cant do queued messages because they are only for consistant protocols
        //HTTP cant because it can only do one message at a time (for now)
        if(protocol != "UDP" && protocol != "HTTP"){
            if(protocol == "TCP"){
                message += "|" + String.Join("|", messageQueue);
                messageQueue = new List<string>();
            }
        }

        if(protocol != "HTTP"){ //http cant do the symbol "|", but it doesnt need it since there is no chance of smashing messages
            message += "|";
        }


        //per protocol sending
        if (protocol == "UDP")
        {
            sendUDPMessage(message);
            sentBytesUDP += System.Text.Encoding.UTF8.GetByteCount(message);
            if(debugUDPMessages){
                Debug.Log("(Client) Sent UDP: " + message);
            }
        }
        else if (protocol == "TCP")
        {
            sendTCPMessage(message);
            sentBytesTCP += System.Text.Encoding.UTF8.GetByteCount(message);
            if(debugTCPMessages){
                Debug.Log("(Client) Sent TCP: " + message);
            }
        }
        else if (protocol == "HTTP")
        {
            sendHTTPMessage(message);
            sentBytesHTTP += System.Text.Encoding.UTF8.GetByteCount(message);
            if(debugHTTPMessages){
                Debug.Log("(Client) Sent HTTP: " + message);
            }
        }
        else{
            Debug.LogError("Unknown protocol: " + protocol + "\nMessage: " + message);
        }

        //Debug.Log("Sent message: " + message);
    }
    #endregion

    #region idk
    public void clearHTTPQueue(){
        sendMessage("server~getQueue", "HTTP");
    }

    void Ping()
    {
        if (!webGLBuild)
        {
            udpPingStartTime = Time.time;
            sendMessage("server~ping", "UDP", true);

            tcpPingStartTime = Time.time;
            sendMessage("server~ping", "TCP", true);

            if(clientID != -1){
                if(!udpRecorded){
                    sendMessage("server~saveProtocol", "UDP", false);
                }

                if(!tcpRecorded){
                    sendMessage("server~saveProtocol", "TCP", false);
                }
            }
        }

        httpPingStartTime = Time.time;
        sendMessage("server~ping", "HTTP", true);
    }
    #endregion

    #region protocol initializers

    void initUDP()
    {
        try
        {
            serverEndpoint = new IPEndPoint(IPAddress.Parse(serverIP), serverUdpPort);

            udpClient = new UdpClient();
            udpClient.Connect(serverEndpoint.Address, serverEndpoint.Port);

            udpReciever();
        }
        catch (Exception e)
        {
            Debug.Log("Couldn't start udp client: " + e);
        }
    }

    void initTCP()
    {
        try
        {
            tcpClient = new TcpClient();
            tcpClient.Connect(IPAddress.Parse(serverIP), serverTcpPort);
            tcpStream = tcpClient.GetStream();

            TCPOnline = true;

            tcpRecieveThread = new Thread(new ThreadStart(tcpReciever));
            tcpRecieveThread.Start();

            if(debugBasicMessages){
                Debug.Log("(Client) TCP Online");
            }
        }
        catch (Exception e)
        {
            Debug.Log("Error connecting to server: " + e);
        }
    }

    void initHTTP()
    {
        //nothing needs to be done to start http, but I'm leaving this here for some consistancy
        if(debugBasicMessages){
            Debug.Log("(Client) HTTP Online");
        }
        connectedToHTTP = true;
    }
    #endregion

    #region recievers
    async void udpReciever()
    {
        try
        {
            if(debugBasicMessages){
                Debug.Log("(Client) UDP Online");
            }
            UDPOnline = true;
            while (UDPOnline)
            {
                UdpReceiveResult result = await udpClient.ReceiveAsync();
                string message = Encoding.ASCII.GetString(result.Buffer);

                processMessage(message, "UDP");
            }
        }
        catch (Exception e)
        {
            Debug.LogError("UDP client exception: " + e);
            failedMessages += 1;
            UDPOnline = false;
        }
    }

    private void tcpReciever()
    {
        byte[] buffer = new byte[1024];
        while (TCPOnline)
        {
            try
            {
                int bytesRead = tcpStream.Read(buffer, 0, buffer.Length);
                if (bytesRead > 0)  
                {
                    string receivedData = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                    UM2_Client.Enqueue(() => processMessage(receivedData, "TCP"));
                }
            }
            catch (Exception e)
            {
                if (e.GetType() != typeof(System.IO.IOException)){ //if the client gets closed
                    Debug.Log("(Client) Error receiving tcp data: " + e);
                    failedMessages += 1;
                }
                else if(debugBasicMessages){
                    Debug.Log("(Client) Closed TCP client");
                }
            }
        }
    }
    #endregion

    #region specific message sending
    public void sendTCPMessage(string message)
    {   
        try
        {
            byte[] data = Encoding.ASCII.GetBytes(message);
            tcpStream.Write(data, 0, data.Length);
        }
        catch (Exception e)
        {
            if(e.GetType() != typeof(System.ObjectDisposedException)){
                Debug.LogError("Error sending tcp data: " + e);
                failedMessages += 1;
            }
            else if(debugBasicMessages){
                Debug.Log("(Client) TCP connection closed");
            }
        }
    }

    public void sendUDPMessage(string message)
    {
        //load message
        byte[] udpData = Encoding.ASCII.GetBytes(message);

        //send message
        udpClient.Send(udpData, udpData.Length);
    }

    async void sendHTTPMessage(string message)
    {
        UnityWebRequest request = UnityWebRequest.Get("http://" + serverIP + ":" + serverHttpPort + "/" + message);
        request.SetRequestHeader("Accept", "application/json");

        // Send the request asynchronously
        //Debug.Log("Sent: " + "http://" + serverIP + ":" + serverHttpPort + "/" + message);
        var operation = request.SendWebRequest();

        // Wait for the request to complete
        while (!operation.isDone)
        {
            await Task.Yield();
        }

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("HTTP error: " + request.error);
            failedMessages += 1;
            return;
        }
        else
        {
            // Print the response data
            string response = request.downloadHandler.text;
            connectedToHTTP = true;
            processMessage(response, "HTTP");
        }
    }

    #endregion

    #region message processing
    void processMessage(string message, string protocol)
    {
        string initialMessage = message;
        if(message == ""){
            return;
        }

        //if there are combined messages (seperated by '|')
        if(message.IndexOf("|") != -1){
            string[] messages = message.Split("|");
            foreach(string singleMessage in messages){
                if(singleMessage != ""){
                    processMessage(singleMessage, protocol);
                }
            }
            return;
        }

        if (protocol == "UDP")
        {
            gotBytesUDP += System.Text.Encoding.UTF8.GetByteCount(message);
            if(debugUDPMessages){
                Debug.Log("(Client) Got UDP: " + message);
            }
        }
        else if (protocol == "TCP")
        {
            gotBytesTCP += System.Text.Encoding.UTF8.GetByteCount(message);
            if(debugTCPMessages){
                Debug.Log("(Client) Got TCP: " + message);
            }
        }
        else if (protocol == "HTTP")
        {
            gotBytesHTTP += System.Text.Encoding.UTF8.GetByteCount(message);
            if(debugHTTPMessages){
                Debug.Log("(Client) Got HTTP: " + message);
            }
        }
        else{
            Debug.LogError("Unknown protocol: " + protocol + "\nI have no idea how this happened lol");
        }

        string methodToCall = message.Split('~')[0];
        //Debug.Log("recieved message: " + message);
        message = message.Substring(methodToCall.Length);
        if(message.Length > 0 && message[0] == '~'){
            message = message.Substring(1);
        }

        string[] messageParts;
        if(message == ""){
            messageParts = new string[] {};
        }
        else{
            messageParts = message.Split("~");
        }

        UM2_Methods.invokeNetworkMethod(methodToCall, messageParts);
        return;
    }

    public void pong(string protocol){
        if (protocol == "UDP")
        {
            udpPing = Time.time - udpPingStartTime;
            connectedToUDP = true;
            //debugger.setDebug("UDP Ping", (int)(udpPing * 1000) + "ms");
        }
        else if (protocol == "TCP")
        {
            tcpPing = Time.time - tcpPingStartTime;
            connectedToTCP = true;
            //debugger.setDebug("TCP Ping", (int)(tcpPing * 1000) + "ms");
        }
        else if (protocol == "HTTP")
        {
            httpPing = Time.time - httpPingStartTime;
            //debugger.setDebug("HTTP Ping", (int)(httpPing * 1000) + "ms");
        }
    }
    #endregion

    #region weird thread stuffs
    //run TCP things on the main thread (while being executed on a different thread)
    private static readonly Queue<Action> actions = new Queue<Action>();
    private static readonly object queueLock = new object();

    private void Update()
    {
        lock (queueLock)
        {
            while (actions.Count > 0)
            {
                actions.Dequeue().Invoke();
            }
        }
    }

    public static void Enqueue(Action action)
    {
        lock (queueLock)
        {
            actions.Enqueue(action);
        }
    }
    #endregion
}