+++
title = "isLocal"
weight = 0
+++
## isLocal
<small>*Namespace: SynicSugar.P2P*</small>

public bool isLocal


### Description
This just return true, if the OwnerUserID is the same with p2pConfig.Instance.userIds.LocalUserId.<br>
Local means that this NetworkPlayer is operated by the this LocalPlayer.<br>
We write the actions for us to perform as the instance owner in this flag.


```cs
using SynicSugar.P2P;
using UnityEngine;

[NetworkPlayer]
public partial class p2pSample {
    [Rpc]
    public void SendMessage(string message){
        if(isLocal){ 
            Debug.Log("Send message");
            return;
        }

        Debug.Log($"{message} from {OwnerUserID}");
    }
}
```