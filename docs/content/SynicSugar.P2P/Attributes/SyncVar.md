+++
title = "SyncVar"
weight = 2
+++
## SyncVar
<small>*Namespace: SynicSugar.P2P* </small>

[AttributeUsage(AttributeTargets.Field, Inherited = false)]<br>
public sealed class SyncVarAttribute : Attribute


### Description
**Under testing and not optimized.**<br>
When values are changed, synchronize with other peers automatically.<br>
If not set syncIntervalMs, use *[p2pConfig.autoSyncInterval](../p2pConfig/autosyncinterval)*.<br>

SyncVar can synchronize built-in type and class serialized by MemoryPack can be synchronized. This has SendToAll in the generated property, so if we want to send MemoryPack's class, it need to be changed with = new().

Internally, this uses SendToAll().<br>
For NetworkPlayer and NetworkCommons.<br>
For NetworkCommons, can synchronize only Host value.


### Constructor

| API | description |
|---|---|
| SyncVar() |  |
| SyncVar(bool isOnlyHost) | For Commons. Sync Host's value with others |
| SyncVar(bool isOnlyHost, int syncIntervalMs) | For Commons. Sync Host's value with others. Sync at a different interval from p2pConfig. |
| SyncVar(int syncIntervalMs) | Sync at a different interval from p2pConfig. |



```cs
using SynicSugar.P2P;
using MemoryPack;
using UnityEngine;

[NetworkCommons]
public class NetworkSample {
    //Only Host send this value in every 3 seconds.
    [SyncVar(true, 3000)]
    int syncIntVar;
    //Sync in every syncIntervalMs
    [SyncVar]
    Status EnemyStatus;
    [SyncVar(true)]
    public Vector3 enemyPos;

    public void ChangeEnemyPos(){
        //Sync 
        syncIntVar = 1;
        //Sync
        enemyPos = new Vector3(1f, 4f, 5f);
        //Not Sync
        enemyPos.x = 2f;
    }
}


[MemoryPackable]
public partial class Status {
    public string Name;
    public int HP;
    public int PP;
}
```