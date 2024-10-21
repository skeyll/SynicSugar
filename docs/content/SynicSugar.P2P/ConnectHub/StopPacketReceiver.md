+++
title = "StopPacketReceiver"
weight = 3
+++
## StopPacketReceiver
<small>*Namespace: SynicSugar.P2P* <br>
*Class: ConnectHub* </small>

public void StopPacketReceiver()


### Description
Stop just getting a packet from the receiving buffer. **To get a packet again, call *[StartPacketReceiver](../ConnectHub/startpacketreceiver)***.<br>


```cs
using SynicSugar.P2P;

public class p2pSample {
    void ConnectHubSample(){
        ConnectHub.Instance.StopPacketReceiver();
    }
}
```