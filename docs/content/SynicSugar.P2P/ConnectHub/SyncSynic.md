+++
title = "SyncSynic"
weight = 11
+++
## SyncSynic
<small>*Namespace: SynicSugar.P2P* <br>
*Class: ConnectHub* </small>

public async void SyncSynic(UserId targetId, SynicType type, byte syncedPhase = 9, bool syncSinglePhase = false)

```cs
    public enum SynicType {
        /// <summary>
        /// Only sender data
        /// </summary>
        OnlySelf,
        /// <summary>
        /// Sender data and (Host) TargetData
        /// </summary>
        WithTarget,
        /// <summary>
        /// Sender data, (Host) TargetData and (Host) Disconencted user Data
        /// </summary>
        WithOthers
    }
```

### Description
Synchronize all own *[Synic](../../Attributes/synic)* variables at once to TargetUser.<br>

targetId: Synced target<br>
type: OnlySelf-Send Local Data, WithTarget-**Host** send target(Reconnecter)'s data, WithOthers-**Host** send target(Reconnecter)'s and other disconencted users' data.
syncedPhase: Sync varaibles from 0 to passed value or only passed value.<br>
syncSinglePhase: If true, sync only varaibles on Synic that have syncedPhase.<br>

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