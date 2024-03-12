### This is a work in progress, but it should contain correct info (but not all info)

**Basics:**
- All Universal Multiplayer scripts start with the prefix UM2_
- 


**Network Methods:**
A way for one client to call another client's method

How to call a network method:
- UM2_Methods.networkMethod*Recipients*(*MethodName*, *recipientIDIfDirect*, parameters[])
 - recipients types:
  - server - if you want to send a message to the server, otherwise it will not ready any of the messages
  - others - sends to all other clients
  - all - sends to all clients (including the sender)
  - direct - sends to a specific client ID

How to set up a network method:
- subscribe to call list
 - change MonoBehaviour to MonoBehaviourUM2 (recommended)
 - subscribe directly with "UM2_Methods.addToServerMethods(this);"


**Global Methods:**
Global methods are similar to Start, but are based on server events. 

OnConnect(int clientID):
- gets called when connection to server is secured
- clientID is your ID

OnPlayerJoin(int clientID)
- gets called when another client joins
- clientID is the joined client's ID

OnPlayerLeave(int clientID)
- gets called when another client leaves
- clientID is the ID of the client that left


If you want to use global methods:
- change MonoBehaviour to MonoBehaviourUM2
- has to be one of the previously shown methods
- needs to be a "public virtual void" 
