+++
title = "Synic"
weight = 5
+++
## Synic
<small>*Namespace: SynicSugar.P2P* </small>

[AttributeUsage(AttributeTargets.Field, Inherited = false)]<br>
public sealed class SynicAttribute : Attribute


### Description
**This is experimental now.**<br>
Synchronize all own Synic variables at once to TargetUser. We can set 0 - 9 as the sync phase can be set, and all variables below the phase or only variables in the specified phase are synchronized at once. This can send larger packets than 1170. To sync, call *[SyncSynic](../../SynicSugar.P2P/ConnectHub/syncsynic)*.<br>

One of the greatest features of Synic is that Host user can synchronize the reconnecter's variables in the local with the recconecter instance data in Host's local. <br>
SynicSugar can synchronize the data only from the owner instance to the same owner Instance. In other words, in order to send the data the reconnecter, Host must get the reconnecter data in the owner instance, and send these via Host's instance RPC that assigns the data in target Player instance.<br>
With Synic, however, Host can synchronize data between the Host's local data and the target local data by simply calling SyncSynic. If the user is not reconnecter, the packets to synchronize data will be discarded.<br>
Maximum packet limit is 296960(1160*256) bytes. When the packetes is over this, the RPC will cause an error in sending it.


*Note: Currently a variables cannot have SyncVar and Synic at the same time.*<br>
*If an error occurs due to around namespace, we need write the full path(like System.CollectionGeneric.AAA())  and it will pass. I will fix this issue in future.*

#### Conditions for Synic
・Has public.<br>
・Can be serialized by JsonUtility. *Detail is [Script serialization](https://docs.unity3d.com/2021.3/Documentation/Manual/script-Serialization.html)*.


### Constructor

| API | description |
|---|---|
| SynicAttribute(byte syncedHierarchy = 0) | Sync all own Synic variables at once to TargetUser. |


```cs
using SynicSugar.P2P;
using MemoryPack;
using UnityEngine;

[NetworkCommons]
public class NetworkCommonsSample {
    //=0
    [Synic] public int turnCount;
    [Synic(1)] public DataStruct dataStruct;
}

[NetworkPlayer]
public class NetworkSample {
    [Synic] public string name;
    [Synic] public int hp;
    [Synic(1)] public Vector3 pos;
    [Synic(1)] public int pp;
    [Synic(3)] public BaddyInfo baddyInfo;

    public void Sync(){
        //Send turnCount, dataStruct, name, hp, pos, pp
        ConnectHub.Instance.SyncSynic(p2pInfo.Instance.LastDisconnectedUsersId, 1);
    }
}

[System.Serializable]
public class DataStruct{
    public int npcDeathCount;
    public string currentBossName;
}

[System.Serializable]
public class BaddyInfo {
    public string Name;
    public int HP;
}
```