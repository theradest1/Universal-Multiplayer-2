# Universal Multiplayer Documentation
This is a work in progress, but it should contain correct info (but not all info)

<br></br>
## **Basics:**
- All Universal Multiplayer scripts start with the prefix UM2_
- UM2_Object based object is referring to the original version of a network object
- UM2_Prefab based object is reffering to the clone of the original network object
- don't use any special characters for anything - stick to letters and numbers
  - HTTP (webgl builds) aren't able to transmit many special characters

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

