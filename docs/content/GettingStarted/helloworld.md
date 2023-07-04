+++
title = "Hello World"
weight = 2
+++
## Hello World

### 1. Sign in EOS

At first, we put EOSManager.prefab in SynicSugar/Runtime/Prefabs/ to Scene. SynicSugar connects with EOS via the scripts on EOSManager. EOSManager automatically do everything around EpicOnlineServices. This Prefab needs all scenes.

![image](https://user-images.githubusercontent.com/50002207/230759934-0d32e507-7194-4783-8b6c-c666d0685b50.png)

To singin EOS, we use EOSAuthentication.LoginWithDeviceID. This has two return value type of bool for just result and string for result details.

SynicSugar is written in (UniTask's) Async/Await. After successful login, can get true or "Success".
```cs
using Cysharp.Threading.Tasks;
using System.Threading;
using SynicSugar.Auth;
public class YourLoginClass : MonoBehaviour {
    async UniTask LoginWithDeviceID(){
        CancellationTokenSource cancellationToken = new CancellationTokenSource();
        bool isSuccess = await EOSAuthentication.LoginWithDeviceID(cancellationToken);
        //or (if need resultCode)
        // string resultCode = await EOSAuthentication.LoginWithDeviceID(cancellationToken, true);
        //if(resultCode == "Success"){ 
        //     //Success
        //     return;
        // }

        if(isSuccess){
            //Success
            return;
        }
        //Failure
    }
}
```     
We can also write without Async/Await processing.

This is so for other all parts. If we can use task harder, we can write codes as usual or put .Forget(). in the last of a function.  However, in this case, the next process is started without waiting for the SynicSugar asynchronous processing.
```cs
// using Cysharp.Threading.Tasks;
using System.Threading;
using SynicSugar.Auth;
public class YourLoginClass : MonoBehaviour {
    public void LoginWithDeviceID(){
        CancellationTokenSource cancellationToken = new CancellationTokenSource();
        
        EOSAuthentication.Instance.LoginWithDeviceID(cancellationToken);
        // or can explicitly ignore Await to get the little more performance.
        // EOSAuthentication.Instance.LoginWithDeviceID(cancellationToken).Forget();
        
        //Continue to process not to wait for login.
        // ...
    }
 }
```      

### 2. Matchmaking

We need ConnectManager for MatchMake and P2P. Put the object from SynicSugar/Core/Prefabs/ConnectManager.prefab to the Matching scene. This object is a singleton and won't be destroyed until explicitly disposed of it.
If you want a scene to correspond to online and offline quantities, you can generate and use it from Prefab. In this case, attach the object to the component.
```cs
using UnityEngine;
using SynicSugar.MatchMake;
public class MatchMaker : MonoBehaviour {
    //Attach on Editor from SynicSugar/Core/Prefabs/ConnectManager
    [SerializeField] GameObject ConnectManagerPrefab;
    void Awake(){
        if(isOnlineMode){ //Your game's mode flag
            Instantiate(ConnectManagerPrefab);
        }
    }
}
```
MatchMakeManager of ConnectConfig has several configs for MatchMake and p2p. We use MatchMakeManager.Instance.XXX to set these config. We can also configure the Unity editor.

![image](https://user-images.githubusercontent.com/50002207/230761023-3754a4fc-46ae-4d33-8f86-9439ce1846c0.png)

To start matchmake, we call MatchMakeManager.Instance.SearchAndCreateLobby(Lobby condition, CancellationTokenSource token). 


Let's make a condition for matchmaking. At first, we need LobbyObject. We can create it by MatchMakeManager.Instance.GenerateLobbyObject(string[] bucket, uint MaxPlayers = 2) and this returns *Lobby*. This bucket is important section for matching like as Region, Mode, Map and so on in [Lobby Interface | Epic Online Services Developer](https://dev.epicgames.com/docs/en-US/game-services/lobbies). However, on current C# sdk, the bucket is just one serarch condition. Create string[] and set the condition that seem important.
We can set other several configs during matchmaking, such as the behavior of the buttons in matchmaking. See MatchMake.cs in Samples for details.

```cs
using SynicSugar.MatchMake;

public class MatchMakeConditions : MonoBehaviour {
    public Lobby GetLobbyCondition(){
        //Create lobby for conditions
        Lobby lobbyCondition = MatchMakeManager.GenerateLobbyObject(new string[3]{"NA", "Rank", "Sea"});
        
        lobbyCondition.MaxLobbyMembers = 2; //2-64
        
        //Add conditions as attributes.
        //The limit of conditions is 100.
        //Need "Key", "Value", and "ComparisonOption"
        LobbyAttribute attribute = new LobbyAttribute();
        attribute.Key = "Level";
        attribute.SetValue(1); //Can use int, string, double, bool
        attribute.comparisonOption = Epic.OnlineServices.ComparisonOp.Equal; // https://dev.epicgames.com/docs/en-US/game-services/lobbies#comparison-operators
        lobbyCondition.Attributes.Add(attribute);
        
        //attribute = new LobbyAttribute();
        //attribute.Key = "SeaMap";
        //...
        
        return lobbyCondition;
    }
}
```
After creating the conditions, call MatchMakeManager.Instance.SearchAndCreateLobby() with it.

This process searches lobby on EOS server, then If there is a Lobby that meets the requirements, will try to join it. If not, creates new lobby as Lobby's Host.
In both pattern, wait until the Lobby is full. If matchmaking is successful, the information necessary for p2p is automatically exchanged.

```cs
using System.Threading;
using Cysharp.Threading.Tasks;
using SynicSugar.MatchMake;
public class MatchMaker : MonoBehaviour {
    async UniTask StartMatching(){
        CancellationTokenSource matchCancellToken = new CancellationTokenSource();
        
        bool isSuccess = await MatchMakeManager.Instance.SearchAndCreateLobby(matchConditions.GetLobbyCondition(), matchCancellToken);
        
        if(isSuccess){
            //Success
            //Move to game scene or un-display the UIs for matchmaking.
            return;
        }
        //Failure
    }
}
```

### 3. p2p connection

We can already connect with other peers. All that remains is to create components to send and receive packets.
SynicSugar has two type network objects: "NetworkPlayer" and "NetworkCommons". NetworkPlayer is like NetworkBehavior in other libraries. This each instance has UserID and, the instance is synchronized with the instance owner's process over the network. On the other hand, NetworkCommons has no UserID. All peers can call a process in this class to synchronize with others. (Or just lobby Host can force to synchronize a process with others.)  We can use NetworkCommons for game system such as game time, NPC HP, item drops and so on.
For both types, we need to create a class with the "public **partial** class" keyword and "NetworkPlayer" or "NetworkCommons" attribute. 
```cs
using SynicSugar.P2P;
[NetworkCommons]
public partial class SampleClass {
    ///
}

using SynicSugar.P2P;
[NetworkPlayer]
public partial class SampleClass {
    ///
}
```

In this tutorial, we implement a function to send HelloWorld on debug log of other peers.
We choice NetworkPlaye to identify who sent it. And use Rpc to send it to the others.
The function to synchronize the process should be public. And we add Rpc attribute to it.
```cs
using UnityEngine;
using SynicSugar.P2P;
[NetworkPlayer]
public partial class PlayerClass : MonoBehaviour {
    [Rpc]
    public void DebugHelloWorld(){
        Debug.Log($"Hello World");
    }
}
```
Rpc function can send a message together with 1st args. We can hard-code "HelloWorld" in the debug log, but let's send HelloWorld as args. And also display the sender's UserID. This sender is the owner of this instance.
In the case, we need to pass "Hello World" 1st args. So, this RPC function should be called with "Hello World" by another function, button event or so on.

```cs
using UnityEngine;
using SynicSugar.P2P;
[NetworkPlayer]
public partial class PlayerClass : MonoBehaviour {
    void Start(){
        CallSendMessage();
    }

    //Call this from Unity Button, Start, or so on.
    public void CallSendMessage(){
        SendMessage("Hello World");
    }

    [Rpc]
    public void SendMessage(string message){
        //SynicSugar genereates OwnerUserID by SourceGenerator
        Debug.Log($"{message} from {OwnerUserID}");
    }
}
```

In gaming, we instantiate this component. This instances need UserID to be gave by EOS. SynicSugar have various way to set a user id to the instance. The simplest way is to call SynicObject.AllSpawn(Gameobject prefab). This function generates an prefab object with NetworkPlayer components, then set UserID to the all components.

```cs
using UnityEngine;
using SynicSugar.P2P;

public class ManagementClass : MonoBehaviour {
    // Attach components with [NetworkPlayer] like PlayerClass on Editor.
    [SerializeField] GameObject playerPrefab; 
    void Start(){
        SynicObject.AllSpawn(playerPrefab);
    }
}
```
---
For only two players, we can set id manually in Start.
```cs
using UnityEngine;
using SynicSugar.P2P;
[NetworkPlayer]
public partial class PlayerClass : MonoBehaviour {
    bool isLocalPlayer;

    void Start(){
        if(isLocalPlayer){ 
            SetOwnerID(p2pConfig.Instance.userIds.LocalUserId);
        }else{
            SetOwnerID(p2pConfig.Instance.userIds.RemoteUserIds[0]);
        }
        
        CallSendMessage();
    }
    ///
}
```
---
The UserID step has the function to register the Instance to ConnctHub that is hub for sending and receiving. In other words, this is the end of the preparation for synchronization. 
Call ConnectHub.Instance.StartPacketReceiver() to get packets from receiving buffer.

```cs
using UnityEngine;
using SynicSugar.P2P;

public class ManagementClass : MonoBehaviour {
    [SerializeField] GameObject playerPrefab; 
    void Start(){
        SynicObject.AllSpawn(playerPrefab);

        //After generating the instances for receive in the local, start Packet Receiver.
        ConnectHub.Instance.StartPacketReceiver();

        //PlayerClass call CallSendMessage() on Start ofter this process.
    }
}
```
We can send the HelloWorld to other peers!We get success to send HelloWorld!
However, we only want to display the message in other peers' DebugLog, don't need in sender's log. In such case, we can use isLocal to separate the processes.

```cs
using UnityEngine;
using SynicSugar.P2P;
[NetworkPlayer]
public partial class PlayerClass : MonoBehaviour {
    void Start(){
        CallSendMessage();
    }
    
    void CallSendMessage(){
        SendMessage("Hello World");
    }


    [Rpc]
    public void SendMessage(string message){
        //This "isLocal" is generated automatically by SynicSugarSourceGenerator to judge local or remote.
        if(isLocal){ 
            Debug.Log("Send message");
            return;
        }

        Debug.Log($"{message} from {OwnerUserID}");
    }
}
```

If you want to synchronize a structure, you need to serialize it with [MemoryPack](https://github.com/Cysharp/MemoryPack). See sample for more details.