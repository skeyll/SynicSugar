+++
title = "PausePacketReceiver"
weight = 3
+++
## PausePacketReceiver
<small>*Namespace: SynicSugar.P2P* <br>
*Class: ConnectHub* </small>

public void PausePacketReceiver()


### Description
Pause just getting a packet from the receiving buffer. **To get a packet again, call *[StartPacketReceiver](../ConnectHub/startpacketreceiver)***.<br>


```cs
using SynicSugar.P2P;

public class p2pSample {
    void ConnectHubSample(){
        ConnectHub.Instance.PausePacketReceiver();
    }
}
```