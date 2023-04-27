+++
title = "Hello World"
weight = 1
+++
## Hello World

### 1. Sign in

To authenticate EOS and manage the tick, EOSManager is needed. Therefore, first place the EOSManager.prefab in SynicSugar/Runtime/Prefabs/ to the Scene.

![image](https://user-images.githubusercontent.com/50002207/230759934-0d32e507-7194-4783-8b6c-c666d0685b50.png)

SynicSugar are written in UniTask's Async/Await. If the authentication succeeds, it returns True.
```cs
using Cysharp.Threading.Tasks;
using System.Threading;
using SynicSugar.Auth;
public class YourLoginClass : MonoBehaviour {
    async UniTask LoginWithDeviceID(){
        CancellationTokenSource cancellationToken = new CancellationTokenSource();
        bool isSuccess = await EOSAuthentication.LoginWithDeviceID(cancellationToken);

        if(isSuccess){
            //Success
            return;
        }
        //Failure
    }
}
```     
You can also write without Async/Await processing.
```cs
// using Cysharp.Threading.Tasks;
using System.Threading;
using SynicSugar.Auth;
public class YourLoginClass : MonoBehaviour {
    public void LoginWithDeviceID(){
        CancellationTokenSource cancellationToken = new CancellationTokenSource();
        
        EOSAuthentication.Instance.LoginWithDeviceID(cancellationToken);
        // or Explicitly ignore AsyncAwait. Get more performance in IL.
        // EOSAuthentication.Instance.LoginWithDeviceID(cancellationToken).Forget();
        
        //Continue to process not to wait.
        // ...
    }
 }
```      

### 2. Matchmaking

You need EOSp2pManager for matchmaking and P2P communication. Please add the SynicSugar/Core/Prefabs/EOSp2pManager.prefab to the Matching scene. You can also  create this from script. This object is a singleton and won't be destroyed until explicitly disposed of.

```cs
using UnityEngine;
using SynicSugar.MatchMake;
public class MatchMaker : MonoBehaviour {
    GameObject matchmakeContainer;
    void Awake(){
        if(MatchMakeManager.Instance == null){
            matchmakeContainer = Instantiate(matchmakePrefab);
        }
    }
}
```
Several settings can be configured in the Unity editor.

![image](https://user-images.githubusercontent.com/50002207/230761023-3754a4fc-46ae-4d33-8f86-9439ce1846c0.png)

To start matchmaking, you can call StartMatching(Lobby lobbyCondition, CancellationTokenSource token, Action saveFn = null). Firstly, the server's lobby is searched and if there is a Lobby that meets the condition, user join it. If there is no Lobby that meets the condition, user create new lobby. The user waits for TimeoutSec, which is set on EOSp2pManager. If the matching is successful, it returns True, otherwise it returns False.

```cs
using SynicSugar.MatchMake;

public class MatchMakeConditions : MonoBehaviour {
    public Lobby GetLobbyCondition(){
        //Create conditions
        Lobby lobbyCondition = EOSLobbyExtenstions.GenerateLobby("Rank", "ASIA");
        
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
After creating the mathing conditions, call MatchMakeManager.Instance.StartMatchMake().

```cs
using SynicSugar.MatchMake;
public class MatchMaker : MonoBehaviour {
    async UniTask StartMatching(){
        CancellationTokenSource matchCancellToken = new CancellationTokenSource();
        
        bool isSuccess = await MatchMakeManager.Instance.StartMatchMake(matchConditions.GetLobbyCondition(), matchCancellToken);
        
        if(isSuccess){
            //Success
            return;
        }
        //Failure
    }
}
```

### 3. p2p connect

In SynicSugar, there are two types of synchronization: "NetworkPlayer" and "NetworkCommons". NetworkPlayer behaves like NetworkBehavior in other libraries, where each instance has its own user ID and only the user with that ID can change the value. On the other hand, NetworkCommons is shared by all users, and can be used to manage system parts such as game time and enemy HP.

For both types, you need to create a class with the "public partial class" keyword and add "NetworkPlayer" or "NetworkCommons" as an attribute. There are no inheritance requirements.

At first, registers NetworkClass for sync to **ConnectHub**. NetworkPlayer is automatically registered when the OwnerUserID is set. NetworkCommons need call **ConnectHub.Instance.RegisterInstance(this);** in NetworkCommons's class to register manually. Then, call **ConnectHub.Instance.StartPacketReceiver();** to start receiving packets. After that, calls Rpc and changes SyncVar to synchronization with other client.

```cs
using UnityEngine;
using SynicSugar.P2P;
[NetworkPlayer]
public partial class XXX {
    [Rpc]
    public void StartMatching(){
        Debug.Log("Hello World");
    }
    ///
}
[NetworkCommons]
public partial class XXX {
    void Start(){
        ConnectHub.Instance.RegisterInstance(this);
    }
    ///
}
```
If you want to synchronize a structure, you need to serialize it with [MemoryPack](https://github.com/Cysharp/MemoryPack). See sample for more details.