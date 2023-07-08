+++
title = "SetOwnerID"
weight = 2
+++
## isLocal
<small>*Namespace: SynicSugar.P2P*</small>

public void SetOwnerID(UserId id)


### Description
Set UserID to this Instance and register it to ConnectHub.<br>
If we generated the object that has this instance by SynicObject API, the instance has already called this process inside its API. <br>
If we use other way, just New() or GetCompornent(), we need call this on or after Start.<br>

Until this is called, isLocal, etc. will not work. So we need to identify user in some other way and get the UserID from p2pConfig.


```cs
using SynicSugar.P2P;
using UnityEngine;

[NetworkPlayer]
public partial class p2pSample : MonoBehaviour {
    [SerializeField] bool isLoaclPlayer;
    void Start(){
        if(isLocalPlayer){ 
            SetOwnerID(p2pConfig.Instance.userIds.LocalUserId);
        }else{
            SetOwnerID(p2pConfig.Instance.userIds.RemoteUserIds[0]);
        }
    }
}
```