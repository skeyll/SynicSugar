+++
title = "EOSp2p"
weight = 9
+++

## EOSp2p
<small>*Namespace: SynicSugar.P2P*</small>


### Description
**This is used by ILPostProcesser.**<br>
**Basically, this is not intended to be used by Library user.**<br>

If we add the network attribute to a method and a field, We can also call these directly.<br>
This 1st arg's byte is ConnectHub.CHANNELLIST.METHOD_OR_VARIABLE. 2nd arg's byte[] is serialized by MemoryPack.


### Properity
| API | description |
|---|---|
| [SendPacketToAll](../EOSp2p/sendpackettoall) | Send packets to all remote peers |
| [SendPacket](../EOSp2p/sendpacket) | Send a packet to a specific peer |


```cs
using SynicSugar.P2P;
using MemoryPack;
[NetworkPlayer] //For [Rpc]
public class p2pSample {
    void HandSend(){
        EOSp2p.SendPacketToAll((byte)ConnectHub.CHANNELLIST.RPCFuction, MemoryPack.MemoryPackSerializer.Serialize(3)).Forget();
    }

    [Rpc] //Then, SourceGenerator add "RPCFuction" to enum ConnectHub.CHANNELLIST.
    void RPCFuction(int value){
    }
}
```