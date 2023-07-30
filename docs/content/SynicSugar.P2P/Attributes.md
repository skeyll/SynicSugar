+++
title = "Attributes"
weight = 1
+++

## Attributes
<small>*Namespace: SynicSugar.P2P*</small>


### Description
Attributes to Synchronize.


### Attributes
| API | description |
|---|---|
| [NetworkPlayer](../Attributes/networkplayer) | For class for each player to have UserID |
| [NetworkCommons](../Attributes/networkcommons) | For class |
| [SyncVar](../Attributes/syncvar) | Synchronize variable with other users |
| [Rpc](../Attributes/rpc) | Synchronize invoked function with other users |
| [TargetRpc](../Attributes/targetrpc) | Synchronize invoked function with other user |
| [Synic](../Attributes/synic) | Synchronize all Synic variables with other user |


```cs
using SynicSugar.P2P;

[NetworkPlayer]
public class NetworkSample {
    [SyncVar]
    int syncIntVar;

    [Rpc]
    void RPCFuction(){
    }
}
```