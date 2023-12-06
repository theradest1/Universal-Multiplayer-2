using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using System.Threading.Tasks;
using System.Threading;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using System.Collections.Generic;
using Unity.VisualScripting;
using System.Reflection;

public class UM2_Client : MonoBehaviour
{
    public static string serverIP = null;
    public static int serverUdpPort = 5000;
    public static int serverTcpPort = 5001;
    public static int serverHttpPort = 5002;
    public static bool hostingServer = false;
    public static bool webGLBuild;
    public static int clientID;

    IPEndPoint serverEndpoint;
    UdpClient udpClient;
    bool connectedToUDP = false;

    TcpClient tcpClient;
    NetworkStream tcpStream;
    bool connectedToTCP = false;
    Thread tcpRecieveThread;

    bool connectedToHTTP = false;


    float udpPingStartTime;
    float udpPing;
    float tcpPingStartTime;
    float tcpPing;
    float httpPingStartTime;
    float httpPing;

    int sentBytes = 0;
    int receivedBytes = 0;
    int failedMessages = 0;

    public UM2_Server server;
    public Debugger debugger;

    private void Start()
    {
        //get info from menu
        if (serverIP == null || serverUdpPort == 0 || serverUdpPort == 0 || serverTcpPort == 0)
        {
            Debug.LogWarning("Server info not set, pushing back to menu");
            SceneManager.LoadScene("Menu");
            return;
        }

        if (webGLBuild && hostingServer)
        {
            Debug.LogWarning("You cannot host a server on a webgl build");
            SceneManager.LoadScene("Menu");
            return;
        }

        debugger.addSpace("Client:");
        debugger.setDebug("UDP Ping", "n/a");
        debugger.setDebug("TCP Ping", "n/a");
        debugger.setDebug("HTTP Ping", "n/a");
        debugger.setDebug("Bytes/Sec sent", "0");
        debugger.setDebug("Bytes/Sec recieved", "0");
        debugger.setDebug("Failed/Sec", "0");

        if (hostingServer)
        {
            debugger.addSpace();
            debugger.addSpace("Server:");
            debugger.setDebug("UDP status", "offline");
            debugger.setDebug("TCP status", "offline");
            debugger.setDebug("HTTP status", "offline");
            debugger.setDebug("Bytes/Sec sent ", "0");
            debugger.setDebug("Bytes/Sec recieved ", "0");
            debugger.setDebug("Failed/Sec ", "0");
        }

        if (!hostingServer) //client gets started by server so it doesnt try to join before server is up
        {
            StartClient();
        }
        else
        {
            server.StartServer();
        }

        InvokeRepeating("updateDebug", 1f, 1f);
    }

    public void join(){
        sendMessage("join", true);
    }

    public void sendMessage(string message, bool reliableProtocol){
        if(connectedToTCP){
            sendMessage(message, "TCP");
        }
        else{
            sendMessage(message, "HTTP");
        }
    }


    void updateDebug()
    {
        debugger.setDebug("Bytes/Sec sent", sentBytes + "");
        debugger.setDebug("Bytes/Sec recieved", receivedBytes + "");
        debugger.setDebug("Failed/Sec", failedMessages + "");

        sentBytes = 0;
        receivedBytes = 0;
        failedMessages = 0;
    }

    public void StartClient()
    {
        if (!webGLBuild)
        {
            initTCP();
            initUDP();
        }
        initHTTP();

        InvokeRepeating("Ping", 0, .25f);
    }

    void Ping()
    {
        if (!webGLBuild)
        {
            udpPingStartTime = Time.time;
            sendMessage("ping", "UDP");

            tcpPingStartTime = Time.time;
            sendMessage("ping", "TCP");
        }

        httpPingStartTime = Time.time;
        sendMessage("ping", "HTTP");
    }

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

            connectedToTCP = true;

            tcpRecieveThread = new Thread(new ThreadStart(tcpReciever));
            tcpRecieveThread.Start();
        }
        catch (Exception e)
        {
            Debug.Log("Error connecting to server: " + e.Message);
        }
    }

    void initHTTP()
    {
        //do some stuff with http
    }

    async void udpReciever()
    {
        try
        {
            while (true)
            {
                UdpReceiveResult result = await udpClient.ReceiveAsync();
                string message = Encoding.ASCII.GetString(result.Buffer);

                processMessage(message, "UDP");
            }
        }
        catch (Exception e)
        {
            Debug.Log("UDP client exception: " + e);
            failedMessages += 1;
        }
    }

    private void tcpReciever()
    {
        byte[] buffer = new byte[1024];
        while (connectedToTCP)
        {
            try
            {
                int bytesRead = tcpStream.Read(buffer, 0, buffer.Length);
                if (bytesRead > 0)
                {
                    string receivedData = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                    MainThreadDispatcher.Enqueue(() => processMessage(receivedData, "TCP"));
                }
            }
            catch (Exception e)
            {
                Debug.Log("Error receiving data: " + e.Message);
                failedMessages += 1;
            }
        }
    }

    public void sendTCPMessage(string message)
    {
        if (connectedToTCP)
        {
            try
            {
                byte[] data = Encoding.ASCII.GetBytes(message);
                tcpStream.Write(data, 0, data.Length);
            }
            catch (Exception e)
            {
                Debug.Log("Error sending data: " + e.Message);
                failedMessages += 1;
            }
        }
    }

    public void stopTCP()
    {
        connectedToTCP = false;

        if (tcpStream != null)
            tcpStream.Close();

        if (tcpClient != null && tcpClient.Connected)
            tcpClient.Close();
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
            Debug.LogError("Error: " + request.error);
            failedMessages += 1;
        }
        else
        {
            // Print the response data
            string response = request.downloadHandler.text;
            processMessage(response, "HTTP");
        }
    }

    public void sendMessage(string message, string protocol)
    {

        if ((protocol == "UDP" || protocol == "TCP") && webGLBuild)
        {
            Debug.LogError(protocol + " cannot be used in a webGL build. Message trying to send was " + message);
            failedMessages += 1;
            return;
        }

        if (protocol == "UDP")
        {
            sendUDPMessage(message);
        }
        else if (protocol == "TCP")
        {
            sendTCPMessage(message);
        }
        else if (protocol == "HTTP")
        {
            sendHTTPMessage(message);
        }

        sentBytes += System.Text.Encoding.UTF8.GetByteCount(message);
    }

    void processMessage(string message, string protocol)
    {
        if(message == ""){
            return;
        }

        //if there are combined messages (seperated by '|')
        if(message.IndexOf("|") != -1){
            string[] messages = message.Split("|");
            foreach(string singleMessage in messages){
                processMessage(singleMessage, protocol);
            }
            return;
        }

        receivedBytes += System.Text.Encoding.UTF8.GetByteCount(message);
        string methodToCall = message.Split('~')[0];
        message = message.Substring(methodToCall.Length);

        string[] messageParts;
        if(message == ""){
            messageParts = new string[] {};
        }
        else{
            messageParts = message.Split("~");
        }



        // Example function to be called dynamically
        // Modify this function based on the function you want to call
        MethodInfo methodInfo = this.GetType().GetMethod(methodToCall);

        if (methodInfo != null)
        {
            ParameterInfo[] parameters = methodInfo.GetParameters();

            // Check if the number of parameters matches the number of tokens
            if (parameters.Length == messageParts.Length + 1)
            {
                object[] parsedParameters = new object[messageParts.Length + 1];

                for (int i = 0; i < messageParts.Length; i++)
                {
                    Type parameterType = parameters[i].ParameterType;
                    object parsedValue = ParseValue(messageParts[i], parameterType);
                    parsedParameters[i] = parsedValue;
                }
                parsedParameters[parsedParameters.Length - 1] = protocol;

                // Call the function dynamically with parsed parameters
                methodInfo.Invoke(this, parsedParameters);
            }
            else
            {
                Debug.LogError("Number of parameters does not match: " + methodToCall + "(" + parameters.Length + "), " + message);
            }
        }
        else
        {
            Debug.LogError("Function not found: \n" + methodToCall + "\"");
        }
    }

    public void pong(string protocol){
        if (protocol == "UDP")
        {
            udpPing = Time.time - udpPingStartTime;
            debugger.setDebug("UDP Ping", (int)(udpPing * 1000) + "ms");
        }
        else if (protocol == "TCP")
        {
            tcpPing = Time.time - tcpPingStartTime;
            debugger.setDebug("TCP Ping", (int)(tcpPing * 1000) + "ms");
        }
        else if (protocol == "HTTP")
        {
            httpPing = Time.time - httpPingStartTime;
            debugger.setDebug("HTTP Ping", (int)(httpPing * 1000) + "ms");
        }
    }

    
    //a bunch of helper methods (make them static and put in a seperate script later-----------------
    public void callFunctionByName(string functionName, params object[] parameters)
    {
        // Get the Type of the current class
        Type type = this.GetType();

        // Get the MethodInfo of the function by name
        MethodInfo methodInfo = type.GetMethod(functionName);

        if (methodInfo != null)
        {
            // Invoke the method with the specified parameters
            methodInfo.Invoke(this, parameters);
        }
        else
        {
            Debug.LogError("Function " + functionName + " not found!");
        }
    }

    private object ParseValue(string token, Type type)
    {
        if (type == typeof(int))
        {
            return int.Parse(token);
        }
        else if (type == typeof(float))
        {
            return float.Parse(token);
        }
        else if (type == typeof(bool))
        {
            return bool.Parse(token);
        }
        else if (type == typeof(string))
        {
            return token;
        }
        else
        {
            throw new ArgumentException("Unsupported parameter type: " + type);
        }
    }
}
