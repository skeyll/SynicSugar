# SynicSugar
![https://github.com/skeyll/SynicSugar/blob/main/LICENSE](https://img.shields.io/github/license/skeyll/SynicSugar) ![Unity](https://img.shields.io/badge/Unity-2021.3%2B-blue)
### What is SynicSugar?
SynicSugar is the syntax sugar to synchronize a game via the internet. The introduce and the process is super sonic. The goal is an easy online-game dev for everyone!

```csharp
using SynicSugar.P2P;
using MemoryPack;
using UnityEngine;
[NetworkPlayer]
public partial class Player {    
    [SyncVar(3000)] public Vector3 pos;
    //Sync in manager's interval
    [SyncVar] public int Hp;
    [SyncVar(1000)] Skill skill;
    
    [Rpc] //Can send 1st parameter to other clients
    public void Attack(int a, int b = 0, float c = 0){
         Hp -= a;
    }
    [TargetRpc] //Can send 2nd parameter to id's client
    public void GreetTarget(UserId id){
         Debug.log("Hi");
    }
    [Rpc] 
    public void Heal(HealInfo info){
         Debug.log($"{OwnerUserId} heal {info.target} {info.amount}");
    }
}    
[MemoryPackable]
public partial class Skill {
    public string Name;
    public bool isValid;
    public int Damage;
}
[MemoryPackable]
public partial class HealInfo {
    public UserId target;
    public int amount;
}
```
```csharp
using SynicSugar.P2P;
using UnityEngine;
[NetworkCommons]
public partial class GameSystem : MonoBehaviour {
    //Sync Host's value
    [SyncVar(true, 500)] public float currentTime;
    [SyncVar] Vector3 enemyPos;
    
    [Rpc] 
    public void StartGame(){
         Debug.log("Start");
    }
}
```

### Feature
- Max 64 peers connction
- MatchMake with your conditions
- Host-Migration
- Re-connect to a disconnected match
- Full-Mesh connct 
- Cross-platform connction (Android, iOS, Windows, Console)
- No cost and No CCU Limit

### Requirement
- Unity 2021-3 or later
- [UniTask](https://github.com/Cysharp/UniTask)
- [MemoryPack](https://github.com/Cysharp/MemoryPack)
- [eos_plugin_for_unity](https://github.com/PlayEveryWare/eos_plugin_for_unity)

 SynicSugar uses SourceGenerator in RoslynGenerator supported by 2021.3 or later. SourceGenerator generates almost all codes for p2p connect on compile automatically.
 
 Large dependencies is for performance. SynicSugar is a full-mesh p2p. All peers connect with each other instead of 1-to-many like dedicated server and client-server model. If we want to sync data with 63 peer in a full-mesh, we need to send data 63 times. Individual connection is fast but the whole is costly. So the core needs faster.

### Getting started
1.Install SynicSugar and dependent librarys via OpenUPM or import asset package (that contains dependencies)  in SynicSugar/release.

2.Get some tokens for EOS. About more, please check the eos document or the plugin page. SynicSugar doesn't need EOS store brand. App credential needs p2p.

3.auth

4.matchmake

5.p2p connect

### Warning
The Rpc process (like SynicSugarRpcxxx) also are generated now. However, I'll change the such send method to insert of IL weaving to prevent unintentional bugs and cheating. You can call the Rpc process manually, but it may change.

### Which netcode is best?
 SynicSugar is a free cost and a easy use. However, it is not perfect. 
While it has standard host-migration, re-connection, and good basic matchmake, p2p cannot completely prevent cheating and full-mesh cannot send to more peers over the fps in a second. SynicSugar is suited for action and battle games in a small group.

 If you create a large scale MMO or BattleRoyale, you can use [Mirror](https://github.com/MirrorNetworking/Mirror). The library to use dedicated server is able to connect hundreds of player. It is also possible to use EOS as a free relay. [Mirage](https://github.com/MirageNet/Mirage) is Mirror with UniTask.
 
 If you need more various API and support, you can select PhotonFusion. The ExitGames's service has provided relays and network SDK for Unity community in many years. Photon is similar to SynicSugar in that it doesn't require managing a server, but the client-host architecture supports over 100 person pvp.
 
 Appropriate netcode depends on the game and team. Your choice determines the path.
