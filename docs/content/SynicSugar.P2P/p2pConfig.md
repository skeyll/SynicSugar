+++
title = "p2pConfig"
weight = 0
+++

## p2pConfig
<small>*Namespace: SynicSugar.P2P*</small>

This is used like **p2pConfig.Instance.XXX()**.


### Description
This is config class for p2p.<br>

This script is Mono's Singleton attached to NetworkManager. To generate NetworkManager, right-click on the Hierarchy and click SynicSugar/NetworkManager.<br>
NetworkManager has **DontDestroy**, so ConnectManager will not be destroyed by scene transitions. <br>

If this is no longer needed, we call *[CancelCurrentMatchMake](../../SynicSugar.MatchMake/MatchMakeManager/cancelcurrentmatchmake)*, *[ConnectHub.Instance.CloseSession(CancellationTokenSource)](../../SynicSugar.P2P/ConnectHub/closesession)* or *[ConnectHub.Instance.ExitSession(CancellationTokenSource)](../../SynicSugar.P2P/ConnectHub/exitsession)*.


### Properity
| API | description |
|---|---|
| [packetReliability](../p2pConfig/packetreliability) | The delivery reliability of a packet |
| AllowDelayedDelivery | Whether users that no connection with local user receives or discards this local user's packet. |
| [UseDisconnectedEarlyNotify](../p2pConfig/usedisconnectedearlynotify) | Notify at the step of a connection interrupted |
| [RPCBatchSize](../p2pConfig/rpcbatchsize) | Frequency of sending RPC in a frame |
| [LargePacketBatchSize](../p2pConfig/largepacketbatchsize) | Frequency of sending LargePacket(Target)RPC in a frame |
| [SynicReceiverBatchSize](../p2pConfig/burstreceivebatchsize) | Frequency of getting (Synic) packet |
| [autoSyncInterval](../p2pConfig/autosyncinterval) | Sending new value interval of SyncVar |
| [AutoRefreshPing](../p2pConfig/autorefreshping) | If true, update ping automatically |
| [SamplesPerPing](../p2pConfig/samplesperping) | Number of samples used for a ping |
| [PingAutoRefreshRateSec](../p2pConfig/pingautorefreshratesec) | Interval sec to update ping automatically |
| [RelayControl](../p2pConfig/relaycontrol) | The way to connection |


### Function 
| API | description |
|---|---|
| [SetRelayControl](../MatchMakeManager/setrelaycontrol) | Change the way to connection |



```cs
using SynicSugar.P2P;

public class p2pConfigManager {
    void Setp2pConfig(){
        p2pConfig.Instance.interval_sendToAll = 10;
    }
}
```