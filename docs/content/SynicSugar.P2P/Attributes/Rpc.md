+++
title = "Rpc"
weight = 3
+++
## Rpc
<small>*Namespace: SynicSugar.P2P* </small>


[AttributeUsage(AttributeTargets.Method, Inherited = false)]<br>
public sealed class RpcAttribute : Attribute


### Description
Invoke method in local and other peers' instance.<br>

Can send 1st argument (that can serialize with [MemoryPack](https://github.com/Cysharp/MemoryPack).<br>
When argument is over 1170 bytes, make true the first argument of RPC. SynicSugar can send it up to 296960 Bytes (about 300KB) as LargePacket. <br>
Normally, RPC is sent to one peer per frame, but by *[p2pConfig.RPCBatchSize](../../SynicSugar.P2P/p2pConfig/rpcbatchsize)*, it can be sent to multiple peers within the same frame.<br>

EOS has Packet reliability, but the packet can not be reached to disconnected peers. When shouldRecordLastPacketInfo is true, can get payload info from p2pInfo that packets that must be sent.<br>
*[ConnectHub.Instance.ResendLastRPC](../../SynicSugar.P2P/ConnectHub/resendlastrpc)*,
*[ConnectHub.Instance.ResendLastRPCToTarget](../../SynicSugar.P2P/ConnectHub/resendlastrpctotarget)*<br>
*If an error occurs due to around namespace, we need write the full path(like System.CollectionGeneric.AAA())  and it will pass. I will fix this issue in future.*


### Constructor

| API | description |
|---|---|
| Rpc() | Standard RPC |
| Rpc(bool isLargePacket, bool shouldRecordLastPacketInfo) | Exceed 1170 bytes? Record latest payload? |



```cs
using SynicSugar.P2P;

[NetworkPlayer]
public partial class NetworkSample {
    [Rpc]
    public void StandardRpc(string message){
    }
    [Rpc(true, false)]
    public void LargePacketRpc(string message){
    }
    [Rpc(false, true)]
    public void ImportantRpc(string message){
    }
}
```