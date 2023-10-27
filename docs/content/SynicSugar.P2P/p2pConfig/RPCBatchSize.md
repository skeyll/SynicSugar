+++
title = "RPCBatchSize"
weight = 1
+++
## RPCBatchSize
<small>*Namespace: SynicSugar.P2P* <br>
*Class: p2pConfig* </small>

public int RPCBatchSize

This is used like **p2pConfig.Instance.XXX()**.


### Description
The number of target users to be sent packet of RPC in a frame. Wait for a frame after one batch. <br />
The sending buffer is probably around 64 KB, so it should not exceed this. If we set 0 from the script, it will cause crash.

Can set this value on UnityEditor.


```cs
using SynicSugar.P2P;

public class p2pConfigManager {
    void Setp2pConfig(){
        p2pConfig.Instance.RPCBatchSize = 5;
    }
}
```