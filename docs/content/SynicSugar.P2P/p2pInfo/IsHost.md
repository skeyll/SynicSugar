+++
title = "IsHost"
weight = 5
+++
## IsLoaclUser
<small>*Namespace: SynicSugar.P2P* <br>
*Class: p2pInfo* </small>

public bool IsHost ()<br>
public bool IsHost (UserId targetId) <br>
public bool IsHost (string targetId) <br>

### Description
If local or target user is host, return true<br>
NetworkCommons has this same process in each class, so this is for other classes.


```cs
using SynicSugar.P2P;
using UnityEngine;
[NetworkPlayer]
public class p2pSample : MonoBehaviour {
    [SyncVar] int gameTime;

    public void CountGameTime(){
        if(!p2pInfo.Instance.IsHost()){
            return;
        }
        gameTime++;
    }
}
```