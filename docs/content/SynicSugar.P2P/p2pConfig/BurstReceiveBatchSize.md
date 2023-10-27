+++
title = "BurstReceiveBatchSize"
weight = 1
+++
## BurstReceiveBatchSize
<small>*Namespace: SynicSugar.P2P* <br>
*Class: p2pConfig* </small>

public int BurstReceiveBatchSize

This is used like **p2pConfig.Instance.XXX()**.


### Description
Frequency of BurstFPS's GetPacket in a frame.<br><br>
If want to receive multiple times during a frame, PerSecondBurstFPS is recommended. When a game has a large number of peers, set the game to 60FPS and *[BurstReceiveBatchSize](../p2pConfig/burstreceivebatchsize)* to 5 to receive. It can get a packet 300 times per second. BurstMode will wait until the next Frame if there are no packets or after getting packets in BatchSize.<br>
If we set 0 from the script, it will cause crash.<br>
*[GetPacketFrequency](../p2pConfig/getpacketfrequency)*

Can set this value on UnityEditor.


```cs
using SynicSugar.P2P;

public class p2pConfigManager {
    void Setp2pConfig(){
        p2pConfig.Instance.BurstReceiveBatchSize = 5;
    }
}
```