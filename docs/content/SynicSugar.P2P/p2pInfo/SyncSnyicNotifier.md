+++
title = "SyncSnyicNotifier"
weight = 4
+++
## SyncSnyicNotifier
<small>*Namespace: SynicSugar.P2P* <br>
*Class: p2pInfo* </small>

public SyncSnyicNotifier SyncSnyicNotifier


### Description
Invoke the registereds event when synced some Synic variables by *[SyncSynic](../../SynicSugar.P2P/ConnectHub/syncsynic)*.<br>

### Properity
| API | description |
|---|---|
| SyncedSynic | Invoke when some Synic variables is synced. |


### Function
| API | description |
|---|---|
| Register | Set events |


```cs
using SynicSugar.P2P;
using UnityEngine;

public class p2pSample : MonoBehaviour {
    void Start(){
        p2pInfo.Instance.SyncSnyicNotifier.SyncedSynic += OnSyncedSynic;
    }

    void OnSyncedSynic(){
        if(!p2pInfo.Instance.HasReceivedAllSyncSynic){
            return;
        }
        Debug.Log($"Get all basis data on phase {p2pInfo.Instance.SyncedSynicPhase}");
    }
}
```