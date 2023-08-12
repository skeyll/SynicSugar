+++
title = "GetNATType"
weight = 9
+++
## GetNATType
<small>*Namespace: SynicSugar.P2P* <br>
*Class: p2pInfo* </small>

public NATType GetNATType()


### Description
Get last-queried NAT-type, if it has been successfully queried.<br>
Type detail is *[here](https://dev.epicgames.com/docs/api-ref/enums/eos-enat-type)*.

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