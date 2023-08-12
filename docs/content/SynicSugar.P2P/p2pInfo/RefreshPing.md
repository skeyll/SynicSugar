+++
title = "RefreshPing"
weight = 11
+++
## RefreshPing
<small>*Namespace: SynicSugar.P2P* <br>
*Class: p2pInfo* </small>

public async UniTask RefreshPing()


### Description
Refresh ping with other all peers. <br>

```cs
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using SynicSugar.P2P;

public class p2pSample : MonoBehaviour {
    public async UniTask RefreshPingManually(){
        await p2pInfo.Instance.RefreshPing();

        ping.text = $"{p2pConfig.Instance.LastConnectedUsersId}: {p2pInfo.Instance.GetPing(p2pConfig.Instance.LastConnectedUsersId)}";
    }
}
```