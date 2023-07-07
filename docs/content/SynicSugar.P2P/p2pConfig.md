+++
title = "p2pConfig"
weight = 0
+++

## p2pConfig
<small>*Namespace: SynicSugar.P2P*</small>

This is used like **p2pConfig.Instance.XXX()**.


### Description
This is configs for p2p. The Singleton instance has UserIDs (of Lobby members) and some config on conenction.<br>

This script is Mono's Singleton attached to ConnenctManager. Drop this **ConnenctManager** into the scene from *Packages/SynicSugar/Runtime/Prefabs/ConnectManager*. <br>
ConnectManager has **DontDestroy**, so ConnectManager will not be destroyed by scene transitions. <br>

If this is no longer needed, we call *~~[CancelCurrentMatchMake](../../SynicSugar.MatchMake/MatchMakeManager/cancelcurrentmatchmake)~~*, *[ConnectHub.Instance.CloseSession(CancellationTokenSource)](../../SynicSugar.P2P/ConnectHub/exitsession)* or *[ConnectHub.Instance.ExitSession(CancellationTokenSource)](../../SynicSugar.P2P/ConnectHub/exitsession)*.


### Properity
| API | description |
|---|---|
| [userIds](../userids/) | UserIDs list of all users in Lobby |
| [interval_sendToAll](../p2pConfig/intervalsendtoall) | Sending to each users interval of Rpc |
| [autoSyncInterval](../p2pConfig/autosyncinterval) | Sending new value interval of SyncVar |
| [receiveInterval](../p2pConfig/receiveinterval) | Frequency of calling PacketReceiver |
| [packetReliability](../p2pConfig/packetreliability) | The delivery reliability of a packet |


```cs
using SynicSugar.P2P;

public class p2pConfigManager {
    void Setp2pConfig(){
        p2pConfig.Instance.interval_sendToAll = 10;
    }
}
```