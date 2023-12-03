+++
title = "CurrentRemoteUserIds"
weight = 1
+++
## CurrentRemoteUserIds
<small>*Namespace: SynicSugar.P2P* <br>
*Class: p2pInfo* </small>

public List<UserId> CurrentRemoteUserIds;


### Description
UserIDs list of this connection. This Id is unique to be assigned by EOS.<br>

```cs
using SynicSugar.P2P;
using UnityEngine;
[NetworkPlayer]
public class p2pSample : MonoBehaviour {
    [Synic(5)] public Vector3 pos;
    [Synic(5)] public Vector3 rotation;

    void SyncWithFakeTransformObject(){
        foreach(var id in p2pInfo.Instance.CurrentRemoteUserIds){
            ConnectHub.Instance.SyncSynic(id, 5, true);
        }
    }
}
```