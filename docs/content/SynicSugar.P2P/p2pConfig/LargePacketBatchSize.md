+++
title = "LargePacketBatchSize"
weight = 1
+++
## LargePacketBatchSize
<small>*Namespace: SynicSugar.P2P* <br>
*Class: p2pConfig* </small>

public int LargePacketBatchSize

This is used like **p2pConfig.Instance.XXX()**.


### Description
The number of packets to be sent of a large packet in a frame. Wait for a frame after a set. <br>
The sending buffer is probably around 64 KB, so it should not exceed this. If we set 0 from the script, it will cause crash.<br><br>

Can set this value on UnityEditor.


```cs
using SynicSugar.P2P;

public class p2pConfigManager {
    void Setp2pConfig(){
        p2pConfig.Instance.LargePacketBatchSize = 5;
    }
}
```