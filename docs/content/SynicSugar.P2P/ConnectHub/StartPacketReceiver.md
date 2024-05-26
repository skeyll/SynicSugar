+++
title = "StartPacketReceiver"
weight = 2
+++
## StartPacketReceiver
<small>*Namespace: SynicSugar.P2P* <br>
*Class: ConnectHub* </small>

public void StartPacketReceiver(PacketReceiveTiming receiveTiming = PacketReceiveTiming.Update, byte maxBatchSize = 1)

```cs
    public enum PacketReceiveTiming {
        FixedUpdate, Update, LateUpdate
    }
```


### Description
Start to get a packet from the receiving buffer.<br>
Need the Instance of each Class to get the packet.<br>
maxBatchSize is how many times during 1 FPS are received. If no packet, skip getting packet until the next frame.<br>
So, genarete the components by *[SynicObject](../synicobject/)*, or set UserID in or after Unity.Start to Register the instanced to ConnectHub.<br>
NetworkComponents need call *[RegisterInstance](../ConnectHub/registerinstance)* in or after Unity.Start by hand.


```cs
using SynicSugar.P2P;

public class p2pSample {
    void ConnectHubSample(){
        ConnectHub.Instance.StartPacketReceiver();
    }
}
```