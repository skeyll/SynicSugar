+++
title = "QueryNATType"
weight = 8
+++
## QueryNATType
<small>*Namespace: SynicSugar.P2P* <br>
*Class: p2pInfo* </small>

public async UniTask QueryNATType()


### Description
Update local user's NATType to the latest state.


```cs
using Cysharp.Threading.Tasks;
using UnityEngine;
using SynicSugar.P2P;

public class p2pSample : MonoBehaviour {
    public async UniTask UpdateNatType(){
        await p2pInfo.Instance.QueryNATType();

        Debug.Log(p2pInfo.Instance.GetNATType());
    }
}
```