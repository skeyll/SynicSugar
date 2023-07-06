+++
title = "packetReliability"
weight = 4
+++
## packetReliability
<small>*Namespace: SynicSugar.P2P* <br>
*Class: p2pConfig* </small>

public PacketReliability packetReliability


### Description
Setting to control the delivery reliability of this packet. <br>
Check [EOS document](https://dev.epicgames.com/docs/api-ref/enums/eos-e-packet-reliability) for more details.

Can set this value on UnityEditor.


```cs
using SynicSugar.P2P;

public class p2pConfigManager {
    void Setp2pConfig(){
        p2pConfig.Instance.packetReliability = Epic.OnlineServices.P2P.PacketReliability.UnreliableUnordered
    }
}
```