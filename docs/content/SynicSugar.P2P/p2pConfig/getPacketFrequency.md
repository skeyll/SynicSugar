+++
title = "getPacketFrequency"
weight = 3
+++
## getPacketFrequency
<small>*Namespace: SynicSugar.P2P* <br>
*Class: p2pConfig* </small>

public GetPacketFrequency getPacketFrequency 


```cs
    public enum GetPacketFrequency {
        PerSecondBurstFPS, PerSecondFPS, PerSecond100, PerSecond50, PerSecond25
    }
```

### Description
Frequency of getting packet in 1sec.<br>
Whichever we choose, the frequency can't exceed the FPS of the game. Excessive receiving will result in a drop in FPS. <br><br>
If want to receive multiple times during a frame, PerSecondBurstFPS is recommended. When a game has a large number of peers, set the game to 60FPS and *[BurstReceiveBatchSize](../p2pConfig/burstreceivebatchsize)* to 5 to receive. It can get a packet 300 times per second. BurstMode will wait until the next Frame if there are no packets or after getting packets in BatchSize.<br>
**PerSecond50 or PerFPS is recommended for mobile game.**<br>
Can set this value on UnityEditor.


```cs
using SynicSugar.P2P;

public class p2pConfigManager {
    void Setp2pConfig(){
        p2pConfig.Instance.getPacketFrequency = p2pConfig.GetPacketFrequency.PerSecond50;
    }
}
```