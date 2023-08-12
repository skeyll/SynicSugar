+++
title = "GetPing"
weight = 10
+++
## GetPing
<small>*Namespace: SynicSugar.P2P* <br>
*Class: p2pInfo* </small>

public int GetPing(UserId id)


### Description
Get the ping with a peer from cache.<br>
The ping with a disconnected user is -1.

```cs
using UnityEngine;
using UnityEngine.UI;
using SynicSugar.P2P;

public class p2pSample : MonoBehaviour {
    public Text ping;
    public void DisplayPing(){
        ping.text = $"{OwnerUserId}: {p2pInfo.Instance.GetPing(OwnerUserId)}";
    }
}
```