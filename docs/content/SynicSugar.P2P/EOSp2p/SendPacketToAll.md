+++
title = "SendPacketToAll"
weight = 0
+++
## SendPacketToAll
<small>*Namespace: SynicSugar.P2P* <br>
*Class: EOSp2p* </small>

public static async UniTaskVoid SendPacketToAll(byte ch, byte[] value)


### Description
Send to a packet to all peers in Lobby.

**This is used by ILPostProcesser.**<br>
**Basically, this is not intended to be used by others.**<br>


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