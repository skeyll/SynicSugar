+++
title = "SendPacket"
weight = 1
+++
## SendPacket
<small>*Namespace: SynicSugar.P2P* <br>
*Class: EOSp2p* </small>

public static void SendPacket(byte ch, byte[] value, UserId targetId)


### Description
Send to a packet to a specific peer in Lobby.

**This is used by ILPostProcesser.**<br>
**Basically, this is not intended to be used by others.**<br>


```cs
using SynicSugar.P2P;
using MemoryPack;
[NetworkPlayer] //For [TargetRpc]
public class p2pSample {
    void HandSend(){
        EOSp2p.SendPacket((byte)ConnectHub.CHANNELLIST.TargetRPCFuction, MemoryPack.MemoryPackSerializer.Serialize("HELLO"), attackUserId).Forget();
    }

    [TargetRpc] //Then, SourceGenerator add "RPCFuction" to enum ConnectHub.CHANNELLIST.
    void TargetRPCFuction(UserID id, string value){
    }
}
```