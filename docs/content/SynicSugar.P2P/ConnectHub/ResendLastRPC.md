+++
title = "ResendLastRPC"
weight = 12
+++
## ResendLastRPC
<small>*Namespace: SynicSugar.P2P* <br>
*Class: ConnectHub* </small>

public void ResendLastRPC()


### Description
Re-Send RPC to All connection peers with last recorded information.<br>
This RPC payload is the same as the last RPC.<br>
To send disconnected peers after some time. **SynicSugar retransmit to connecting-peers like TCP.**<br>
To record, pass true to attribute.

```cs
using SynicSugar.P2P;
using UnityEngine;

public class p2pSample : MonoBehaviour {
    public void ResendRPC() {
        if(p2pInfo.Instance.LastRPCch != (byte)ConnectHub.CHANNELLIST.ImportantSyncProcess){
            return;
        }
        ConnectHub.Instance.ResendLastRPC();
    }
}
```