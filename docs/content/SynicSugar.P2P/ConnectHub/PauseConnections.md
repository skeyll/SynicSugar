+++
title = "PauseConnections"
weight = 4
+++
## PauseConnections
<small>*Namespace: SynicSugar.P2P* <br>
*Class: ConnectHub* </small>

public async UniTask PauseConnections(bool isForced = false, CancellationTokenSource cancelToken = default(CancellationTokenSource))


### Description
Stop receiving packets to the receive buffer and getting a packet from there.<br>
After call this, the packets will have been discarded until connection will re-open.<br>
To getting restart, call [RestartConnections](../ConnectHub/restartconnections). 

If 1st is false, get all packets in current buffer, then stop receiving.<br>
If true, stop receiving a packet to the buffer immediatly.

2nd token is just for this Task instead of PacketReceiver's.


**WARNING: This doesn't work as intended now. Can't stop receiving packets to buffer, so SynicSugar discard those packets before re-start.**


```cs
using SynicSugar.P2P;
using Cysharp.Threading.Tasks;

public class p2pSample {
    void ConnectHubSample(){
        ConnectHub.Instance.PauseConnections().Forget();
    }
}
```