+++
title = "RefreshPing"
weight = 11
+++
## RefreshPing
<small>*Namespace: SynicSugar.P2P* <br>
*Class: p2pInfo* </small>

public async UniTask RefreshPing(UserId target)


### Description
Refresh ping with target. <br>
We can't call RefleshPings at the same time until the process is finished.

```cs
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using SynicSugar.P2P;

public class p2pSample : MonoBehaviour {
    public async UniTask RefreshPingManually(){
        await p2pInfo.Instance.RefreshPing(p2pConfig.Instance.LastConnectedUsersId);

        ping.text = $"{p2pConfig.Instance.LastConnectedUsersId}: {p2pInfo.Instance.GetPing(p2pConfig.Instance.LastConnectedUsersId)}";
    }
}
```