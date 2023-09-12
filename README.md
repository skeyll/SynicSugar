# [<img src="https://github.com/skeyll/SynicSugar/blob/main/Resources/Logo_doc.png" width="40%">](https://skeyll.github.io/SynicSugar/)
# SynicSugar
![https://github.com/skeyll/SynicSugar/blob/main/LICENSE](https://img.shields.io/github/license/skeyll/SynicSugar) ![Unity](https://img.shields.io/badge/Unity-2021.3%2B-blue) [![openupm](https://img.shields.io/npm/v/net.skeyll.synicsugar?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/net.skeyll.synicsugar/) 

SynicSugar is Unity High-Level Network Library with EpicOnlineServices. The concept is the syntax sugar of netcode. Matchmaking, Relay and VC are for free thanks to Epic Game, and have high-level APIs for actual game development. Almost SynicSugar APIs are zero-allocation, so the runtime process is sonic. SynicSugar will be optimized for small-party game not covered by [Mirror](https://github.com/MirrorNetworking/Mirror).

For more detail is [https://skeyll.github.io/SynicSugar/](https://skeyll.github.io/SynicSugar/).


## Feature
 - Mesh topology with max 64 peers
 - No Use Cost and No CCU Limit
 - High-level APIs for mobile and small-group games (MatchMaking, Host-Migration, To sent Large-packet, PushToTalk, and Re-connection...)
 - Almost all RPC processes are zero-allocation
 - Free VoiceChat
 - Cross-platform connction <br>
    (Current: Android, iOS, and PC / Preview: Console / Future: WebGL)


```csharp
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

## Requirement
- Unity 2021-3 or later
- [UniTask](https://github.com/Cysharp/UniTask)
- [MemoryPack](https://github.com/Cysharp/MemoryPack)
- [eos_plugin_for_unity](https://github.com/PlayEveryWare/eos_plugin_for_unity)
- [Mono.Cecil](https://github.com/jbevain/cecil)

 SynicSugar uses Roslyn SourceGenerator supported after 2021.3. SourceGenerator generates almost all codes for p2p connect on compile automatically.  
 Large dependencies is for performance. SynicSugar is mesh-topology p2p. All peers connect with each other instead of 1-to-many like dedicated server and client-server model. If we want to sync data with 63 peer in a full-mesh, we need to send data 63 times. Individual connection is fast but the whole is costly. So the core needs faster.  

## Getting started
### 1.Install SynicSugar and depended librarys.  
 The first is to import SynicSugar and dependent libraries.　You can get SynicSugar from OpenUPM or [SynicSugar/Release](https://github.com/skeyll/SynicSugar/releases)'s unitypackage.  
 .unitypackage contains Mono.Cecil and System.Runtime.CompilerServices.Unsafe.dll for MemoryPack and, in addition to SynicSugar. Therefore, you can skip some processes, but it is more convenient to download via OpenUPM for version control.  

1. Rigister some package with OpenUPM  

 In your unity project, select Edit/ProjectSetting/PackageManager. Then, register some librarys.
 
 Name: OpenUPM
 
 URL: https://package.openupm.com
 
 Scope(s):
* net.skeyll.synicsugar (Skip if downloading as unitypackage)
* com.cysharp.unitask
* com.playeveryware.eos
* com.cysharp.memorypack
           
![image](https://user-images.githubusercontent.com/50002207/230567095-04cfbfcc-f1c9-4b0d-9088-2fbfc08da8f8.png)


2. Install these packages  
　These packages can be imported from **Window/PackageManager/MyRegistries**. Importing SynicSugar will automatically import the other required librarys. If you are using another version in your project, that one will probably work. However, SynicSugar has been developed using the following:  
 * Epic Online Services Plugin for Unity: 2.2.0  
 * UniTask: 2.3.1 
 * MemoryPack: 1.9.13  
 
 
 3. Import the rest (Skip if downloading as unitypackage.)  
Import what is not in OpenUPM.  
- Mono.Cecil  
Enter **com.unity.nuget.mono-cecil** in **Edit/ProjectSetting/PackageManager/+/Add package from git URL**.  

![image](https://user-images.githubusercontent.com/50002207/231324146-292634b7-3d42-420d-a20c-37f5fc0ad688.png)

- System.Runtime.CompilerServices.Unsafe  
MemoryPack need System.Runtime.CompilerServices.Unsafe.dll. You can get this dll from Download package in https://www.nuget.org/packages/System.Runtime.CompilerServices.Unsafe/6.0.0 . Since this contains DLLs for multiple environments, only import packages for Unity. Unzip the downloaded file and drag and drop **lib/netstandard2.0/System.Runtime.CompilerServices.Unsafe.dll** into your project.


### 2.Get some tokens for EOS.

Please check [the eos document](https://dev.epicgames.com/ja/news/how-to-set-up-epic-online-services-eos) or [the plugin page](https://github.com/PlayEveryWare/eos_plugin_for_unity). SynicSugar doesn't need EOS store brand. Just register and can develop online games.

About app credential, you can use Peer2Peer as ClientPolicy. The minimum is as follows.
![image](https://user-images.githubusercontent.com/50002207/230758754-4333b431-48fe-4539-aa97-20c6f86d68ae.png)


## For Debug
SynicSugar has Debug.Log in some portions for the game development. To use the logs, add SYNICSUGAR_LOG to Scripting Define Symbols in project setting. Logs for errors are displayed by default.


## License
 License is under the MIT.

## Contribute Guideline

SynicSugar's concept is an easy online game development for everyone. Therefore, the development is also based on this policy. We can create online game for up to 64 people supported by EOS, but the main is small-party(2-32) game. If you want to create MMO, Survival Game and Party Game, you should use [Mirror](https://github.com/MirrorNetworking/Mirror). 

The roadmap is [here](https://github.com/users/skeyll/projects/5/views/2). For the time being, add a basic function for online-game and improve the performance. If you need any necessary functions, please post it to Github Issue or give a pull request. Great thanks for all contributions!
