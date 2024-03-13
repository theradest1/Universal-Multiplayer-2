# This is a work in progress, but it should contain correct info (but not all info)

---

## **Basics:**
- All Universal Multiplayer scripts start with the prefix UM2_

---

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

---

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

---

## Network Object (UM2_Object):
- ### prefab
  - what is created for the other clients
  - put the prefab in the resources folder of the UM2 parent folder
- ### object ID 
  - this is just debug, no touchy
- ### Ticks per second:
  - this is how fast the object is synced
  - higher is faster, but more expensive
  - lower is less exact, but less expensive
  - try to do higher for players, and lower for objects
- ### Sync transform:
  - makes the transform (position and rotation) the same across all clients
- ### optimize transform sync
  - uses the minimum ticks per second if there is no changes in the transform
  - I recommend using this on non player objects along with a higher normal ticks per second for smoothness along with performance
- ### Min ticks per second:
  - only used if optimize transform sync is enabled
  - the ticks per second goes down to this if there is no change in transform
  - its good to not have it zero if there isnt a rigidbody on it, otherwise just put it to zero