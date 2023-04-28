+++
title = "SynicSugar"

# The homepage contents
[extra]

# Menu items
[[extra.menu.main]]

[[extra.list]]

+++
# What is SynicSugar?
SynicSugar is the syntax sugar of netcode for Unity with Epic Online Services. The process is sonic by pre-generating and IL weaving codes for your project. The goal is an easy online game dev for everyone! SynicSugar will be optimized for small-party (action) game. Can import from [Github](https://github.com/skeyll/SynicSugar)


```cs
using SynicSugar.P2P;
using UnityEngine;
[NetworkPlayer(true)]
public partial class Player {  
    //Sync every 1000ms(default)
    [SyncVar] public int Hp;
    //Sync every 3000ms
    [SyncVar(3000)] Skill skill;
    
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
}
```

## Feature
 - Max 64 peers full-mesh connect
 - No Use Cost and No CCU Limit
 - MatchMake with your conditions
 - Host-Migration
 - Re-connect to a disconnected match
 - Cross-platform connction (Android, iOS, Windows, and Console)

## Requirement
 - Unity 2021-3 or later
 - [EpicOnlineServices](https://dev.epicgames.com/en-US/services)

## Dependencis
- [UniTask](https://github.com/Cysharp/UniTask)
- [MemoryPack](https://github.com/Cysharp/MemoryPack)
- [eos_plugin_for_unity](https://github.com/PlayEveryWare/eos_plugin_for_unity)
- [Mono.Cecil](https://github.com/jbevain/cecil)

 SynicSugar uses Roslyn SourceGenerator supported after 2021.3. SourceGenerator generates almost all codes for p2p connect on compile automatically.  
Large dependencies is for performance. SynicSugar is a full-mesh p2p. All peers connect with each other instead of 1-to-many like dedicated server and client-server model. If we want to sync data with 63 peer in a full-mesh, we need to send data 63 times. Individual connection is fast but the whole is costly. So the core needs faster.  

## Contribution Guideline
SynicSugar's concept is an easy online game development for everyone. Therefore, the development is also based on this policy. The target is online game for up to 64 people supported by EOS, but the main is small-party action game. If you want to create MMO, you can use [Mirror](https://github.com/MirrorNetworking/Mirror). 
The roadmap is to expand the necessary functions for these games and improve performance. If you need any necessary functions, please post it to Github Issue. Alternatively, if you want to add a necessary function, please give a pull request. Great thanks for all contributions!