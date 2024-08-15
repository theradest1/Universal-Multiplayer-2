# Universal Multiplayer Documentation
This is a work in progress, but it should contain correct info (but not all info)

<br></br>
## **Index:**
- [Install](#install)
- [Quick Start](#quick-start)
- [Basic Info For Reading Docs](#basic-info-for-docs)
- [Tips and Tricks](#tips-and-tricks)
- [Network Variables](#network-variables)
- [Network Methods](#network-methods)
- [Global Methods](#global-methods)
- [Network Objects](#network-objects)
- [Quick Objects](#quick-objects)
- [Animation Sync](#animation-sync)

<br></br>
## **Install:**
import UM2:
- download latest unity package from releases
- right click in explorer in unity
- import package -> custom package
- find and select UM2_Vx.x.x
- click import all

HTTP fix, do this if you want P2P (just do this if you aren't sure):
  - go to edit -> Project settings -> Player -> Other settings -> configuration -> allow downloads over HTTP
  - change "Not Allowed" to "Always allowed"

Look at these before starting your own project:
- [Basic Info For Reading Docs](#basic-info-for-docs) section for random peices of info that is good to know
- [Tips and Tricks](#tips-and-tricks) to see how to see how I recommend you use UM2
- [Quick Start](#quick-start) section for a quick tutorial on how to get players moving around and some explainations

Examples:
- this repository is essentially a big example, but if you want a more accurate-to-life example of how to use UM2, check out my other repository "UM2-Example"
  - it is a multiplayer game that I made using UM2, and is what I use to figure out problems and improvements

Thank you for reading the docs (:

Questions:
- There isnt a fixed place to submit questions currently so just use github issues until I figure things out
- If you found a bug or think something could be improved (including docs), please submit an issue request in github

<br></br>
## **Quick Start:**
This will be how to make a very simple first person multiplayer thing

read and follow ALL of the install information

read it again slowly

ask questions if anything doesn't make sense

I would recommend looking at "Basic info for docs" for good miscilaneous peice of information that could help in the future

If you don't want to go through this, or want an example, the actual unity project in this repo (not the package) uses everything that UM2 has to offer set up already. I would still recommend going through this because it could help explain how some things work.

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
Create a first person controller just like you would in a single player game. I recommend brackey's video on this if you don't know how: https://www.youtube.com/watch?v=_QajrabyTJc

Duplicate your player and remove everything that isn't visual from the duplicated player. Things like movement scripts and the camera should be removed from the duplicated player. The only components that should be left on it is the transform, mesh filter, mesh renderer, and maybe a collider if you want.

Make this into a prefab by dragging it into the project folder panel. Put it in the folder called "Resources" in the "UM2" folder. You can then delete the duplicated player in the scene.

You should now have a working player with a controller in the scene, and a visual copy of that player as a prefab in the folder at Assets -> UM2 -> Resources. The prefab is what the other players/clients will see in place of your local player.

Add the UM2_Object script into your working player game object, and put the visual prefab in the prefab variable in the inspector. Look at the "Network Objects" part of the docs if you want to know more about the other variables.

#### **Connecting**:
Create a new scene that will be used for connecting. Ill let you figure out how to do the menu stuff, but you will need to set a few variables before connecting (loading the scene automatically makes you connect)
- serverIP (string)
- webGLBuild (bool) (automatically false)

You can set these by doing `UM2_Client.{Variable} = {Value};`. For example if I wanted to set the server IP, I would do `UM2_Client.serverIP = "127.0.0.1";`

Currently the multiplayer system that UM2 uses is called P2P, or peer to peer. This means that one of the peers/players/clients will need to also be the server while all of the other peers/players/clients will connect to that server.

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
- players, peers, and clients are all the same thing 
- All Universal Multiplayer scripts start with the prefix UM2_
- UM2_Object based object is referring to the original version of a network object
- UM2_Prefab based object is reffering to the clone of the original network object
- Webgl builds cannot be a server and can only use HTTP
- don't use any special characters for anything - stick to letters and numbers
  - This is because HTTP isn't able to transmit many special characters
- if you want to port forward to connect through WAN, forward with this (find a youtube video on how to port-forward):
  - udp on 5000
  - tcp on 5001
  - http on 5002 (both udp and tcp)
  - these can be changed by doing `UM2_Server.{udp/tcp/http}Port = {port};` on the server and `UM2_Client.server{Udp/Tcp/Http}Port = {port};` on the client during the same time as setting the server ip.
    - I don't recommend doing this unless you know what you are doing
- if a client is connecting to its own server use the ip `127.0.0.1` (local host)


<br></br>
## **Tips and tricks:**
- callbacks are great and should be used when possible, especially since multiplayer is often not consistant with timing
- for testing your game, you can build and run one instance and run in the inspector for another. Make one host a server and the other use the IP `127.0.0.1` (local host)
  - make sure to switch back and forth a lot since some methods only run when the window is focused (I have no control over this)
- you generally don't need any reference to UM2 scripts, just use the name/type of the script
- If there is something that is centralized (like a boss in a game), have the client that is running the server control it. You can do this by checking `UM2_Client.hostingServer`

<br></br>
## **Network Variables**
This allows you to have variables that Two kinds: global and object based

- can only be an int, float, or string (no lists yet)
- both are synced across the network automatically

### Global network variables:
A network variable that can be accessed anywhere by any script, without any references to anything. Don't use this for things like player health, as it isnt linked to anything (use object based network variables)

Each global variable must have a different name

Uses:
- Timer
- Team scores
- Player count

Creating:
- `UM2_Variables.createNetworkVariable<type>(string variableName, object initialValue);`
- Example 1: `UM2_Variables.createNetworkVariable<string>("roomName", "The Room")`
- Example 2: `UM2_Variables.createNetworkVariable<float>("timer", 60f)`

Getting Value:
- `UM2_Variables.getNetworkVariableValue<type>(string name)`
- the `<type>` does a type cast for you so you don't have to do it manualy. Just use the type the network variable is.
- Example: `UM2_Variables.getNetworkVariableValue<float>("timer")`

Setting Value:
- `UM2_Variables.setNetworkVariableValue(string name, object value)`
- Example: `UM2_Variables.setNetworkVariableValue("timer", 120f)`

Adding to Value:
- this is used for variables that might be overwritten by different clients (like score)
- `UM2_Variables.addToNetworkVariableValue(string name, object valueToAdd)`
- Example: `UM2_Variables.addToNetworkVariableValue("timer", 2f)`

Check if variable exists:
- `UM2_Variable.getNetworkVariable(string name)`
- Example: `bool exists = UM2_Variable.getNetworkVariable("timer") != null`
- returns null if a global variable with that name doesn't exist

Setting a callback function:
- `UM2_Variables.addVarCallback(string variableName, Action<object> methodToCall)`
- triggers when the variable's value is set or changed
- the `Action<object>` needs to be a public void function that has `object newValue` as it's parameter (newValue would be the new value of the variable)
- as many callback functions can be added as you want

Example: 
```
void OnConnect(int clientID){
  //create the timer variable as a float
  UM2_Variables.createNetworkVariable<float>("timer", 60f);

  //add the updateGUI function to the callback functions
  UM2_Variables.addVarCallback("roomName", updateGUI);
}

//a public void with a single object parameter
public void updateGUI(object newValue){
  //do something with the timer value
  timerElement.text = newValue + ""; 

  // + "" to change the value into a string
}
```

### Object based network variables:
Everything that you can do in this section with the UM2_Object script can be done with the UM2_Prefab script. For example, getting, setting, and adding to a variable is done in the exact same way with the UM2_Prefab as the UM2_Object.

A network variable that is related to an object. You will need a reference to a game object's UM2_Object script. I recommend doing this with `UM2_Object networkObjectScript = player.GetComponent<UM2_Object>();` (player being the game object)

Uses:
- health
- usernames

Creating:
- must be created in a script that is on an object with UM2_Object script 
- Use the flag [ObjectNetworkVariable] in front of the variable you want to be synced
- example: `[ObjectNetworkVariable] int health;`
- Unity complains when the varible isn't used, so I recommend just doing `variable += 0;` in a function that is never called.

Getting Value:
- networkObjectScript.getNetworkVariableValue(string name)

Setting Value:
- networkObjectScript.setNetworkVariableValue(string name, object value)
- do not set it using the actual variable (it will not be synced)

Adding to Value:
- this is used for variables that might be overwritten by different clients (like a score)
- networkObjectScript.addToNetworkVariableValue(string name, object valueToAdd)

Check if variable exists:
- networkObjectScript.getNetworkVariable(string name)
- returns null if a global variable with that name doesn't exist

Setting a callback function:
- `networkObjectScript.addVarCallback(string variableName, Action<object> methodToCall)`
- triggers when the variable's value is set or changed
- the `Action<object>` needs to be a public void function that has `object newValue` as it's parameter (newValue would be the new value of the variable)
- as many callback functions can be added as you want

Example: 
```
public UM2_Object networkObjectScript;

void OnConnect(int clientID){
  //create the timer variable as a float
  networkObjectScript.createNetworkVariable<float>("timer", 60f);

  //add the updateGUI function to the callback functions
  networkObjectScript.addVarCallback("roomName", updateGUI);
}

//a public void with a single object parameter
public void updateGUI(object newValue){
  //do something with the timer value
  timerElement.text = newValue + ""; 

  // + "" to change the value into a string
}
```

<br></br>
## **Network Methods:**
A way for one client to call another client's method

### How to call a network method:
UM2_Methods.networkMethod\*Recipients\*(string methodName, int recipientIDIfDirect, object parameter1, object parameter2...)


#### recipient types:
- Others - sends to all other clients
- All - sends to all clients (including the sender)
- Direct - sends to a specific client ID
- Server - if you want to send a message to the server, you shouldn't use this unless modifying the server
- Example 1: `UM2_Methods.networkMethodAll(startNewGame, difficulty, timeLimit, totalEnemies)`
- Example 2: `UM2_Methods.networkMethodDirect(chatMessage, clientID, message)`
  - The recipient ID is only there if it is a direct message


The main recipients you will (and should) be using is `Others` and `All`.

### How to create a network method:
#### subscribe to call list
There are two ways:
- change `MonoBehaviour` to `MonoBehaviourUM2` at the top of the script (recommended)
- subscribe directly with `UM2_Methods.addToServerMethods(this);`

#### create the method
It just needs to be a public void that has the same parameters as is called. Here is a reminder that the script that this method is in has to be a `MonoBehaviourUM2` or subscribed directly.

Duplicate functions across scripts will also be called. For example you could put this next example in several scripts, and all will be called (assuming all scripts are a `MonoBehaviourUM2` or subscribed directly)

Example (if the method call is the 2nd example shown above):
```
public void chatMessage(int senderID, string message){
  //do stuff
}
```

<br></br>
## **Global Methods**:
Global methods are similar to `Start`, `Awake`, and `Update`, but are based on server events. 

### All global methods so far:
- #### OnConnect(int clientID):
  - gets called when connection to server is finished
  - clientID is your ID
- #### OnPlayerJoin(int clientID)
  - gets called when another client joins
  - clientID is the joined client's ID
- #### OnPlayerLeave(int clientID)
  - gets called when another client leaves
  - clientID is the ID of the client that left

### How to use global methods:
- change MonoBehaviour to MonoBehaviourUM2
- needs to be a `public override void` method
- needs to have the same parameters as shown above

Example: 
```
public override void OnConnect(int clientID){
 //do stuff 
}
```

<br></br>
## **Network Objects:**
A way to sync the position and rotation of an object.

### Good Uses:
- Players
- Enemies
- Projectiles (like grenades)

### How to use
(follow the quick start section for more in depth steps and better explainations)
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

### Other info:
If you want the object to teleport, such as having a player respawn, you must use the function `objectScript.teleportObject()` right before or after changing the position or rotation of the object. If not used, the other clients will see the player ease to the teleported position.

It doesn't actually set the position, but just stops the network prefabs from easing between this and the next transform update.

Example:
```
public UM2_Object objectScript;

public void respawn(){
  //reset position
  transform.position = Vector3.zero;

  //use function
  objectScript.teleportObject();
}
```

<br></br>
## **Quick Objects:**
Quick objects are a network object that is synced once, then forgotten about. 

### Good uses:
- Bullet (if it is cosmetic)
- Explosion
- Any particle effect

### How to use:
- put the object you want to create in `Assets -> UM2 -> Resources`
- create the quick object with `UM2_Sync.createQuickObject(quickObjectPrefab, transform.position, transform.rotation);`

<br></br>
## **Animation Sync:**
Syncronizes the animation parameters for a network object

### Good uses:
- pretty much any animation that is on a synced object

## How to use:
- Put a UM2_Animator script on a game object with the UM2_Object script
- The Animator component needs to be on the same game object as the UM2_Animator script.
- The prefab only needs to have the Animator component on the base game object

