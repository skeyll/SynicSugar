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
| [interval_sendToAll](../p2pConfig/intervalsendtoall) | Sending to each users interval of Rpc |
| [autoSyncInterval](../p2pConfig/autosyncinterval) | Sending new value interval of SyncVar |
| [GetPacketFrequency](../p2pConfig/getpacketfrequency) | Frequency of calling PacketReceiver |
| [packetReliability](../p2pConfig/packetreliability) | The delivery reliability of a packet |
| [UseDisconnectedEarlyNotify](../p2pConfig/usedisconnectedearlynotify) | Notify at the step of a connection interrupted |
| [FirstConnection](../p2pConfig/firstconnection) | Delay to return true after matchmaking is completed |
| [SamplesPerPing](../p2pConfig/samplesperping) | Number of samples used for a ping |
| [AutoRefreshPing](../p2pConfig/autorefreshping) | If true, update ping automatically |
| [PingAutoRefreshRateSec](../p2pConfig/pingautorefreshratesec) | Interval sec to update ping automatically |


```cs
using SynicSugar.P2P;

public class p2pConfigManager {
    void Setp2pConfig(){
        p2pConfig.Instance.interval_sendToAll = 10;
    }
}
```