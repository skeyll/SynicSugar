+++
title = "ResendLastTargetRPC"
weight = 14
+++
## ResendLastTargetRPC
<small>*Namespace: SynicSugar.P2P* <br>
*Class: ConnectHub* </small>

public void ResendLastTargetRPC()


### Description
Re-Send RPC to the specific target with last recorded information.<br>
This TargetRPC payload is the same as the last TargetRPC.<br>
To send disconnected peers after some time. **SynicSugar retransmit to connecting-peers like TCP.**<br>
To record, pass true to attribute.

```cs
using SynicSugar.P2P;
using UnityEngine;

public class p2pSample : MonoBehaviour {
    public void ResendTargetRPC {
        if(p2pInfo.Instance.LastRPCch != (byte)ConnectHub.CHANNELLIST.ImportantSyncProcess){
            return;
        }
        ConnectHub.Instance.ResendLastTargetRPC();
    }
}
```