# Universal Multiplayer Documentation
This is a work in progress, but it should contain correct info (but not all info)

<br></br>
## **Install:**
- import UM2:
  - download latest unity package from releases
  - right click in explorer in unity
  - import package -> custom package
  - find and select UM2_Vx.x.x
  - import all
- HTTP fix: do this if you want P2P (just do this if you aren't sure):
    - go to edit -> Project settings -> Player -> Other settings -> configuration -> allow downloads over HTTP
    - change "Not Allowed" to "Always allowed"
- Look at the Quick Start section for next steps
- Examples:
  - this repository is essentially a big example, but if you want a more accurate-to-life example of how to use UM2, check out my other repository "UM2-Example"
    - it is a multiplayer game that I made using UM2, and is what I use to figure out problems and improvements
- Thank you for reading the docs (:
- Questions:
  - There isnt a fixed place to submit questions currently so just use github issues until I figure things out
  - If you found a bug or think something could be improved (including docs), please submit an issue request in github

<br></br>
## **Quick Start:**
This will be how to make a first person multiplayer thing as simply as possible  
- read and follow ALL of the install information
  - read it again slowly
  - ask questions if anything doesn't make sense
  - I would recommend looking at "Basic info for docs"

#### **Making the manager**:
Create an empty object in your game scene and name it "UM2 Manager". You can name it other things, but that is what I call it and how I will reference it in the docs.

Add the following scripts to the UM2 Manager. Ignore all of the settings on the scripts for now, but feel free to change them later:
  - UM2_Client
  - UM2_Server
  - UM2_Sync
  - UM2_Variables
  - UM2_Methods
  - UM2_Events

#### **Making the player**:
Create a first person controller just like you would in a single player game. I would recommend brackey's video on this if you don't know how: https://www.youtube.com/watch?v=_QajrabyTJc

Duplicate your player and remove everything that isn't visual. Things like movement scripts and the camera should be deleted off of the duplicated player. The only components that should be left on it is the transform, mesh filter, mesh renderer, and maybe a collider if you want.

Make this into a prefab by dragging it into the project folder panel. Put it in the folder called "Resources" in the "UM2" folder. You can then delete the duplicated player in the scene.

You should now have a working player with a controller in the scene, and a visual copy of that player as a prefab in the folder at Assets -> UM2 -> Resources. The prefab is what the other players/clients will see in place of your local player.

Add the UM2_Object script into your working player game object, and put the visual prefab in the prefab variable in the inspector. Look at the "Network Objects" part of the docs if you want to know more about the other variables.

#### **Connecting**:
Create a new scene that will be used for connecting to multiplayer. Ill let you figure out how to do the menu stuff, but you will need to set a few variables before connecting (loading the scene automatically makes you connect)
- serverIP (string)
- hostingServer (bool) (automatically false)
- webGLBuild (bool) (automatically false)

You can set these by doing `UM2_Client.{Variable} = {Value};`. For example if I wanted to set the server IP, I would do `UM2_Client.serverIP = "127.0.0.1";`

Currently the multiplayer that this uses is called P2P, or peer to peer. This means that one of the peers/players/clients will need to also be the server while all of the other peers/players/clients will connect to that server.

The main reason I made it like this is because having a dedicated server requires you to use money and I don't have money D: (and it is infinitly scalable). In the future there will be one, but for now it is just P2P.

If the client is going to be the server, these functions will need to be called:
- `UM2_Server.GetPublicIPAddress();` 
- `UM2_Server.GetLocalIPAddress();`

The `UM2_Client.hostingServer` variable will also need to be set to `true`

I will reference the client that is hosting the server as server-client. The clients that are connecting to the server-client will be just called client.

For the clients, they will need the IP of the server-client. This can be given to the client by the server-client, and it can be gotten by the server-client by calling the get IP functions, then accessing the IPs with `UM2_Server.publicIpAddress` and `UM2_Server.localIpAddress`. Use the public IP if the server-client and client are on different networks, and use the local IP if they are on the same network. Set server's ip on the client with `UM2_Client.serverIP = {server IP};`

If they are not on the same network, the server-client will also need to port forward. Look at the basic info for more info on that. 

It should be done, and you will see the other player after joining with the correct ip. I say this in the basic info too, but if one of the clients are full screen, the other client doesnt send network messages. This sometimes makes it so the other client doesnt immedietly spawn in, so just make the windows not full screen

<br></br>
## **Basic info for docs:**
- All Universal Multiplayer scripts start with the prefix UM2_
- UM2_Object based object is referring to the original version of a network object
- UM2_Prefab based object is reffering to the clone of the original network object
- Webgl builds cannot be a server and can only use HTTP
- don't use any special characters for anything - stick to letters and numbers
  - This is because HTTP isn't able to transmit many special characters
- callbacks are great and should be used when possible, especially since multiplayer is often not consistant
- if you want to port forward to connect through WAN, forward with this (find a youtube video on how to port-forward):
  - udp on 5000
  - tcp on 5001
  - http on 5002 (both udp and tcp)
- these can be changed by doing `UM2_Server.{udp/tcp/http}Port = {port};` on the server and `UM2_Client.server{Udp/Tcp/Http}Port = {port};` on the client during the same time as setting the server ip.
  - I don't recommend doing this unless you know what you are doing
- for testing your game, you can build and run one instance and run in the inspector for a second. Make one host a server and the other use the IP `127.0.0.1` (local host)
  - if one of the clients are full screen, the other client doesnt send network messages (make both windowed)
- if a client is connecting to its own server use the ip `127.0.0.1` (local host)

<br></br>
## Network Variables
This allows you to have variables that Two kinds: global and object based

### Info for Both:
- can only be an int, float, or string
- both are synced across the network automatically

### Global network Variables:
- uses:
  - a network variable that can be accessed anywhere, without any references to anything
  - timer
  - team scores
- each variable must have a different name
- Creating:
  - UM2_Variables.createNetworkVariable<float>(string variableName, object initialValue, [callbackFunction]);
  - Example 1: UM2_Variables.createNetworkVariable<float>("health", 100f, onHealthChange)
  - Example 2: UM2_Variables.createNetworkVariable<float>("health", 100f)
- Getting Value:
  - UM2_Variables.getNetworkVariableValue(string name)
- Setting Value:
  - UM2_Variables.setNetworkVariableValue(string name, object value)
- Adding to Value:
  - this is used for variables that might be overwritten by different clients (like a score)
  - UM2_Variables.addToNetworkVariableValue(string name, object valueToAdd)
- Check if variable exists:
  - UM2_Variable.getNetworkVariable(string name)
  - returns null if a global variable with that name doesn't exist
- Setting a callback function:
  - triggers when the variable's value is set
  - UM2_Variables.addVarCallback(string variableName, Action<object> methodToCall)

### Object based Variables:
- Uses:
  - a network variable that is related to an object
  - health
  - usernames
- getting, setting, and adding to the value of an object based network variable is the same on a UM2_Object and UM2_Prefab based object (creating can only be done in code on the UM2_Object)
- Creating:
  - must be created in a script that is on an object with UM2_Object script 
  - Use the flag [ObjectNetworkVariable] in front of the variable you want to be synced
  - example: [ObjectNetworkVariable] int health;
- Getting Value:
  - networkObjectScript.getNetworkVariableValue(string name)
- Setting Value:
  - networkObjectScript.setNetworkVariableValue(string name, object value)
  - do not set it using the actual variable (it will not be synced)
- Adding to Value:
  - this is used for variables that might be overwritten by different clients (like a score)
  - networkObjectScript.addToNetworkVariableValue(string name, object valueToAdd)
- Check if variable exists:
  - networkObjectScript.getNetworkVariable(string name)
  - returns null if a global variable with that name doesn't exist

<br></br>
## Network Methods:
A way for one client to call another client's method

### How to call a network method:
- UM2_Methods.networkMethod*Recipients*(*MethodName*, *recipientIDIfDirect*, parameters[])
- #### recipients types:
  - server - if you want to send a message to the server, otherwise it will not ready any of the messages
  - others - sends to all other clients
  - all - sends to all clients (including the sender)
  - direct - sends to a specific client ID

### How to set up a network method:
- #### subscribe to call list
  - change MonoBehaviour to MonoBehaviourUM2 (recommended)
  - subscribe directly with "UM2_Methods.addToServerMethods(this);"

<br></br>
## Global Methods:
Global methods are similar to Start, but are based on server events. 

### All global methods so far:
- #### OnConnect(int clientID):
  - gets called when connection to server is secured
  - clientID is your ID
- #### OnPlayerJoin(int clientID)
  - gets called when another client joins
  - clientID is the joined client's ID
- #### OnPlayerLeave(int clientID)
  - gets called when another client leaves
  - clientID is the ID of the client that left


### How to use global methods:
- change MonoBehaviour to MonoBehaviourUM2
- has to be one of the previously shown methods
- needs to be a "public virtual void"

<br></br>
## Network Object:
A way to sync the position and rotation of an object.

### Good Uses:
- Players
- Projectiles (like grenades)

### How to use
- attach the UM2_Object script onto the object you want to be synced
- create a prefab 
  - this is what will be created for the other players
  - do not have a UM2_Object or UM2_Prefab script on it
  - put it in `Assets -> UM2 -> Resources`
  - reference it in the UM2_Object script
- change the UM2_Object parameters in the inspector:
  - prefab
    - what is created for the other clients
    - put the prefab in `assets -> UM2 -> resources`
  - object ID 
    - this is just debug, no touchy
  - Ticks per second:
    - this is how fast the object is synced
    - higher is faster, but more expensive
    - lower is less exact, but less expensive
    - try to do higher for players, and lower for other objects
  - Sync transform:
    - makes the transform (position and rotation, no scale) the same across all clients
  - optimize transform sync
    - uses the minimum ticks per second if there is no changes in the transform
    - I recommend using this on pretty much everything
  - Min ticks per second:
    - only is used if optimize transform sync is enabled
    - the ticks per second goes down to this if there is no change in transform
    - if there is a rigidbody, do not set this to 0

<br></br>

## Quick Objects:
Quick objects are a network object that is synced once, then forgotten about. 

### Good uses:
- Bullet (if it is cosmetic)
- Explosion
- Any particle effect

### How to use:
- put the object you want to create in `Assets -> UM2 -> Resources`
- create the quick object with `UM2_Sync.createQuickObject(quickObjectPrefab, transform.position, transform.rotation);`

<br></br>

## Animation Sync:
Syncronizes the animation parameters for a network object

### Good uses:
- pretty much any animation that is on a synced object

## How to use:
- 

