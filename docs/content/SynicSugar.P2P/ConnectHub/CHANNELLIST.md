+++
title = "CHANNELLIST"
weight = 1
+++
## CHANNELLIST
<small>*Namespace: SynicSugar.P2P* <br>
*Class: ConnectHub* </small>

public enum CHANNELLIST


### Description
This enum has all elements that have SyncVar, Rpc or TargetRpc.<br>
SourceGenerator automatically add these element on compile. It can hold up to 255 elements in each Assembly.


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