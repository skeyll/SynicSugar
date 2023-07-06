+++
title = "TargetRpc"
weight = 4
+++
## Rpc
<small>*Namespace: SynicSugar.P2P* </small>


[AttributeUsage(AttributeTargets.Method, Inherited = false)]<br>
public sealed class TargetRpcAttribute : Attribute


### Description
On Fire, invoked in target peer.

1st args is target UserID. 2nd can be synchronized.<br>
Anything that can be serialized by [MemoryPack](https://github.com/Cysharp/MemoryPack).


### Constructor

| API | description |
|---|---|
| TargetRpc() |  |



```cs
using SynicSugar.P2P;

[NetworkCommons]
public class NetworkSample {
    [TargetRpc]
    void TargetRPCFuction(UserID id){
    }
}
```