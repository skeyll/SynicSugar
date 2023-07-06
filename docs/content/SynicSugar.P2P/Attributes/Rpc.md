+++
title = "Rpc"
weight = 3
+++
## Rpc
<small>*Namespace: SynicSugar.P2P* </small>


[AttributeUsage(AttributeTargets.Method, Inherited = false)]<br>
public sealed class RpcAttribute : Attribute


### Description
On Fire, invoked in other peers.<br>
1st args can be synchronized.<br>
Anything that can be serialized by [MemoryPack](https://github.com/Cysharp/MemoryPack).

Check *[p2pConfig.interval_sendToAll](../p2pConfig/intervalsendtoall)*.<br>
*This API is most likely a change.*

### Constructor

| API | description |
|---|---|
| Rpc() |  |



```cs
using SynicSugar.P2P;

[NetworkPlayer]
public class NetworkSample {
    [Rpc]
    void RPCFuction(string message){
    }
}
```