+++
title = "IsLoaclUser"
weight = 5
+++
## IsLoaclUser
<small>*Namespace: SynicSugar.P2P* <br>
*Class: p2pInfo* </small>

public bool IsLoaclUser (UserId targetId) <br>
public bool IsLoaclUser (string targetId) <br>

### Description
If target is LocalUser, return true.<br>
NetworkPlayer has this same process in each class, so this is for other classes.


```cs
using SynicSugar.P2P;
using UnityEngine;

public class p2pSample : MonoBehaviour {
    public void Init(UserId id){
        if(!p2pInfo.Instance.IsLoaclUser(id)){
            return;
        }
    }
}
```