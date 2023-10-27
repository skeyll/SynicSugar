+++
title = "TargetRpc"
weight = 4
+++
## TargetRpc
<small>*Namespace: SynicSugar.P2P* </small>


[AttributeUsage(AttributeTargets.Method, Inherited = false)]<br>
public sealed class TargetRpcAttribute : Attribute

### Description
Invoke method in local and other peer's instance. This is only for NetworkPlayer.<br>

1st args is target UserID. 2nd can be synchronized.<br>
Can send 1st argument (that can serialize with [MemoryPack](https://github.com/Cysharp/MemoryPack).<br>
When argument is over 1170 bytes, make true the first argument of RPC. SynicSugar can send it up to 296960 Bytes (about 300KB) as LargePacket. <br>

EOS has Packet reliability, but the packet can not be reached to disconnected peers. When shouldRecordLastPacketInfo is true, can get payload info from p2pInfo that packets that must be sent.<br>
*[ConnectHub.Instance.ResendLastTargetRPC](../../SynicSugar.P2P/ConnectHub/ResendLastTargetRPC)*


### Constructor

| API | description |
|---|---|
| TargetRpc() | Standard RPC |
| TargetRpc(bool isLargePacket, bool shouldRecordLastPacketInfo) | Exceed 1170 bytes? Record latest payload?|



```cs
using SynicSugar.P2P;

[NetworkPlayer]
public partial class NetworkSample {
    [TargetRpc]
    void TargetRPCFuction(UserID id){
    }
}
```