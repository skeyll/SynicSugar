![image](https://github.com/skeyll/SynicSugar/assets/50002207/381e4209-3fbb-415e-9a33-42f24c02538a)# SynicSugar
![https://github.com/skeyll/SynicSugar/blob/main/LICENSE](https://img.shields.io/github/license/skeyll/SynicSugar) ![Unity](https://img.shields.io/badge/Unity-2021.3%2B-blue) [![openupm](https://img.shields.io/npm/v/net.skeyll.synicsugar?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/net.skeyll.synicsugar/)  
SynicSugar is the syntax sugar to synchronize a game via the internet. The backend is EOS, so the server cost is free. The goal is an easy online-game dev for everyone!  

For more detail is [https://skeyll.github.io/SynicSugar/](https://skeyll.github.io/SynicSugar/).


## Feature
- Max 64 peers full-mesh connect
- No Use Cost and No CCU Limit
- MatchMake with your conditions
- Host-Migration
- Re-connect to a disconnected match
- Cross-platform connction (Android, iOS, Windows, and Console)

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
    [Rpc] //Can pass multiple data class with MemoryPack
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
    //Sync Host's value by 500ms
    [SyncVar(true, 500)] public float currentTime;
    [SyncVar] Vector3 enemyPos;
    
    [Rpc] 
    public void StartGame(){
         Debug.log("Start");
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
 Large dependencies is for performance. SynicSugar is a full-mesh p2p. All peers connect with each other instead of 1-to-many like dedicated server and client-server model. If we want to sync data with 63 peer in a full-mesh, we need to send data 63 times. Individual connection is fast but the whole is costly. So the core needs faster.  

## Warning 
 SynicSugar is still in development.　This library is made for my game and will be developed with my game. Therefore, It may bugs and fewer features. And will have destructive changes. So, I currently recommend using [Mirror](https://github.com/MirrorNetworking/Mirror) with [EOSRelay](https://github.com/FakeByte/EpicOnlineTransport) for a product.

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

Please check [the eos document](https://dev.epicgames.com/ja/news/how-to-set-up-epic-online-services-eos) or [the plugin page](https://github.com/PlayEveryWare/eos_plugin_for_unity). SynicSugar doesn't need EOS store brand. Just register and can use server.

About app credential, you can use Peer2Peer as ClientPolicy. The minimum is as follows.
![image](https://user-images.githubusercontent.com/50002207/230758754-4333b431-48fe-4539-aa97-20c6f86d68ae.png)


## For Debug
Some process can show log for debug within If SYNISSUGAR_LOG. We need this log, add SYNICSUGAR_LOG to Scripting Define Symbols in project setting.


## License
 License is under the MIT. I will never change it.

## Contribute Guideline
SynicSugar's concept is an easy online game development for everyone. Therefore, the development is also based on this policy. The target is online game for up to 64 people supported by EOS, but the main is small-party action game. If you want to create MMO, you can use Mirror. The roadmap is to expand the necessary functions for these games and improve performance. If you need any necessary features, please post it to Github Issue, or pull. Great thanks for all contributions!
