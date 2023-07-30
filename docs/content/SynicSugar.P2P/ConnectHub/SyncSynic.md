+++
title = "SyncSynic"
weight = 11
+++
## SyncSynic
<small>*Namespace: SynicSugar.P2P* <br>
*Class: ConnectHub* </small>

public void SyncSynic(UserId targetId, byte syncedPhase = 9, bool syncSinglePhase = false, bool syncTargetsData = true)


### Description
Synchronize all own *[Synic](../Attributes/synic)* variables at once to TargetUser.<br>

targetId: Synced target<br>
syncedPhase: Sync varaibles from 0 to passed value or only passed value.<br>
syncSinglePhase: If true, sync only varaibles on Synic that have syncedPhase.<br>
syncTargetsData: If true, **Host** send target's data too.

```cs
using SynicSugar.P2P;
using UnityEngine;

public class p2pSample : MonoBehaviour {
    void Start() {
        p2pInfo.Instance.ConnectionNotifier.Disconnected += OnDisconect;
        p2pInfo.Instance.ConnectionNotifier.Connected += OnConnected;
    }
    void OnDisconect(){
        chatText.text += $"{p2pInfo.Instance.LastDisconnectedUsersId} is Disconnected / {p2pInfo.Instance.LastDisconnectedUsersReason}{System.Environment.NewLine}";
        
    }
    void OnConnected(){
        chatText.text += $"{p2pInfo.Instance.LastDisconnectedUsersId} Join {System.Environment.NewLine}";
        //Send local data
        ConnectHub.Instance.SyncSynic(p2pInfo.Instance.LastDisconnectedUsersId, 5, false, true);
    }
}
```