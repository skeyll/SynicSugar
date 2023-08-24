+++
title = "ResendLastRPCToTarget"
weight = 13
+++
## ResendLastRPCToTarget
<small>*Namespace: SynicSugar.P2P* <br>
*Class: ConnectHub* </small>

public void ResendLastRPCToTarget(UserId target)


### Description
Re-Send RPC to the specific target with last recorded information.<br>
This TargetRPC payload is the same as the last RPC.<br>
To send disconnected peers after some time. **SynicSugar retransmit to connecting-peers like TCP.**<br>
To record, pass true to attribute.

```cs
using SynicSugar.P2P;
using UnityEngine;

public class p2pSample : MonoBehaviour {
    void Start() {
        p2pInfo.Instance.ConnectionNotifier.Connected += OnConnected;
    }
    public void OnConenction() {
        if(p2pInfo.Instance.LastRPCch != (byte)ConnectHub.CHANNELLIST.ImportantSyncProcess){
            return;
        }
        ConnectHub.Instance.ResendLastRPC(p2pInfo.Instance.LastConnectedUsersId);
    }
}
```