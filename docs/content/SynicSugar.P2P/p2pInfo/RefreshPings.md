+++
title = "RefreshPings"
weight = 11
+++
## RefreshPings
<small>*Namespace: SynicSugar.P2P* <br>
*Class: p2pInfo* </small>

public async UniTask RefreshPings()


### Description
Refresh ping with other all peers. <br>
We can't call RefleshPing at the same time until the process is finished.

```cs
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using SynicSugar.P2P;

public class p2pSample : MonoBehaviour {
    public async UniTask RefreshPingManually(){
        await p2pInfo.Instance.RefreshPings();

        ping.text = $"{p2pConfig.Instance.LastConnectedUsersId}: {p2pInfo.Instance.GetPing(p2pConfig.Instance.LastConnectedUsersId)}";
    }
}
```