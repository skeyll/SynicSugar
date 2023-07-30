+++
title = "LocalUserId"
weight = 0
+++
## LocalUserId
<small>*Namespace: SynicSugar.P2P* <br>
*Class: p2pInfo* </small>

public UserId LocalUserId;


### Description
UserID of this local. This Id are assigned by EOS.<br>

```cs
using SynicSugar.P2P;
using UnityEngine;
[NetworkPlayer]
public class p2pSample : MonoBehaviour {
    public bool isLocalPlayer;

    void Start(){
        if(isLocalPlayer){
            SetOwnerID(p2pInfo.Instance.LocalUserId);
        }
    }
}
```