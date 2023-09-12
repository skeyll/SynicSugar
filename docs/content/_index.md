+++
title = "SynicSugar"

# The homepage contents
[extra]

# Menu items
[[extra.menu.main]]

[[extra.list]]

+++
# What is SynicSugar?
SynicSugar is Unity High-Level Network Library with EpicOnlineServices. The concept is the syntax sugar of netcode. Matchmaking, Relay and VC are for free thanks to Epic Game, and have high-level APIs for actual game development. Almost SynicSugar APIs are zero-allocation, so the runtime process is sonic. SynicSugar will be optimized for small-party game not covered by [Mirror](https://github.com/MirrorNetworking/Mirror). Can get it from [Github](https://github.com/skeyll/SynicSugar).


```cs
using SynicSugar.P2P;
using UnityEngine;
[NetworkPlayer(true)]
public partial class Player {  
    //Sync every 1000ms(default)
    [SyncVar] public int Hp;
    //Sync every 3000ms
    [SyncVar(3000)] Skill skill;

    [Synic(0)] public string name;
    [Synic(0)] public string item;
    [Synic(1)] public Vector3 pos;
    
    [Rpc] 
    public void Attack(int a){
        Hp -= a;
    }
    [TargetRpc]
    public void GreetTarget(UserId id){
        Debug.log("Hi");
    }
    [Rpc]
    public void Heal(HealInfo info){
        Debug.log($"{OwnerUserId} heal {info.target} {info.amount}");
    }

    public void SyncBasisStatus(){
        //Sync the Variables that have Synic(0) attribute at once.
        ConnectHub.Instance.SyncSynic(p2pInfo.Instance.LastDisconnectedUsersId, 0);
    }
}
```

## Feature
 - Mesh topology with max 64 peers
 - No Use Cost and No CCU Limit
 - High-level APIs for mobile and small-group games (MatchMaking, Host-Migration, To sent Large-packet, PushToTalk, and Re-connection...)
 - Almost all RPC processes are zero-allocation
 - Free VoiceChat
 - Cross-platform connction <br>
    (Current: Android, iOS, and PC / InTest: Console / Future?: WebGL)

## Requirement
 - Unity 2021-3 or later
 - [EpicOnlineServices](https://dev.epicgames.com/en-US/services)

## Dependencis
- [UniTask](https://github.com/Cysharp/UniTask)
- [MemoryPack](https://github.com/Cysharp/MemoryPack)
- [eos_plugin_for_unity](https://github.com/PlayEveryWare/eos_plugin_for_unity)
- [Mono.Cecil](https://github.com/jbevain/cecil)

 SynicSugar uses Roslyn SourceGenerator supported after 2021.3. SourceGenerator generates almost all codes for p2p connect on compile automatically.  
Large dependencies is for performance. SynicSugar is a full-mesh p2p. All peers connect with each other instead of 1-to-many like dedicated server and client-server model. If we want to sync data with many peers in a full-mesh, we need to send data 63 times. Individual connection is fast but the whole is costly. So the core needs faster. 

## For Debug
SynicSugar has Debug.Log in some portions for the game development. To use the logs, add SYNICSUGAR_LOG to Scripting Define Symbols in project setting. Logs for errors are displayed by default.

## Contribution Guideline
SynicSugar's concept is an easy online game development for everyone. Therefore, the development is also based on this policy. The target is online game for up to 64 people supported by EOS, but the main is small-party action game. If you want to create MMO, you should use [Mirror](https://github.com/MirrorNetworking/Mirror). 
The roadmap is [here](https://github.com/users/skeyll/projects/5/views/2). For the time being, add basic function for online-game and improve performance. If you need any necessary functions, please post it to Github Issue or give a pull request. Great thanks for all contributions!