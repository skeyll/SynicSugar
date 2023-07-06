+++
title = "SyncVar"
weight = 2
+++
## SyncVar
<small>*Namespace: SynicSugar.P2P* </small>

[AttributeUsage(AttributeTargets.Field, Inherited = false)]<br>
public sealed class SyncVarAttribute : Attribute


### Description
When values are changed, synchronize with other peers automatically.<br>
If not set syncIntervalMs, use *[p2pConfig.autoSyncInterval](../p2pConfig/autosyncinterval)*.

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

[NetworkCommons]
public class NetworkSample {
    [SyncVar(true, 3000)]
    int syncIntVar;
}
```