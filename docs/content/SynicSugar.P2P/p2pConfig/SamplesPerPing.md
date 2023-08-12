+++
title = "SamplesPerPing"
weight = 7
+++
## SamplesPerPing
<small>*Namespace: SynicSugar.P2P* <br>
*Class: p2pConfig* </small>

public byte SamplesPerPing;


### Description
Number of samples to calculate one ping.<br>
**Currently 1 is recommended. This uses the same PacketReceiver for connection. The more samples this need, the larger ping this gets now.**<br>


```cs
using SynicSugar.P2P;

public class p2pConfigManager {
    void Setp2pConfig(){
        p2pConfig.Instance.SamplesPerPing = 10;
    }
}
```