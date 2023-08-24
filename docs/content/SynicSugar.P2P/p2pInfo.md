+++
title = "p2pInfo"
weight = 5
+++

## p2pInfo
<small>*Namespace: SynicSugar.P2P*</small>

This is used like **p2pinfo.Instance.XXX()**.


### Description
This manages data after p2p connections have established.<br>

This script is Mono's Singleton attached to NetworkManager. To generate NetworkManager, right-click on the Hierarchy and click SynicSugar/NetworkManager<br>
NetworkManager has **DontDestroy**, so ConnectManager will not be destroyed by scene transitions. <br>

If this is no longer needed, we call *[CancelCurrentMatchMake](../../SynicSugar.MatchMake/MatchMakeManager/cancelcurrentmatchmake)*, *[ConnectHub.Instance.CloseSession(CancellationTokenSource)](../../SynicSugar.P2P/ConnectHub/exitsession)* or *[ConnectHub.Instance.ExitSession(CancellationTokenSource)](../../SynicSugar.P2P/ConnectHub/exitsession)*.


### Properity
| API | description |
|---|---|
| [LocalUserId](../p2pInfo/localuserid) | UserID of this local |
| [RemoteUserIds](../p2pInfo/remoteuserids) | UserIDs list of this connection |
| [AcceptHostSynic](../p2pInfo/accepthostsynic) | If true, host overwrite this local user instnace |
| [ConnectionNotifier](../p2pInfo/connectionnotifier) | Notifers when a user connects or disconnects |
| LastDisconnectedUsersReason | The reason of last disconnected user |
| LastDisconnectedUsersId | UserID of last Disconnected user |
| LastConnectedUsersId | UserID of last Connected user |
| [SyncSnyicNotifier](../p2pInfo/syncsnyicnotifier) | Notifers when a user get SynicVariables |
| HasReceivedAllSyncSynic | Return True only once after local user receives all SyncSynics |
| SyncedSynicPhase | The SyncSynic phase of last received |
| LastSyncedUserId | The UserID of last synced SyncSynic |
| LastRPCPayload | The last byte array sent with RPC that record data |
| LastRPCch | The last ch sent with RPC that record data |
| LastTargetRPCPayload | The last byte array sent with TargetRPC that record data |
| LastTargetRPCch | The last ch sent with TargetRPC that record data |
| LastTargetRPCUserId | The last UserId sent with TargetRPC that record data |

### Function
| API | description |
|---|---|
| [IsLoaclUser](../p2pInfo/isloacluser) | If target is LocalUser, return true |
| [IsHost](../p2pInfo/ishost) | If local or target user is host, return true |
| [GetCurrentConnectionMemberCount](../p2pInfo/getcurrentconnectionmembercount) | Get the current member count in connection |
| [GetAllConnectionMemberCount](../p2pInfo/getallconnectionmembercount) | Get the all member's count of current and left connection |
| [QueryNATType](../p2pInfo/querynattype) | Update local user's NATType to the latest |
| [GetNATType](../p2pInfo/getnattype) | Get last-queried NAT-type |
| [GetPing](../p2pInfo/getping) | Get a ping with a peer from cache |
| [RefreshPing](../p2pInfo/refreshping) | Refresh ping with other all peers |



```cs
using SynicSugar.P2P;
using UnityEngine;
[NetworkPlayer]
public class p2pSample : MonoBehaviour {
    void Start(){
        if(!isLoacl){
            return;
        }
        
        p2pInfo.Instance.ConnectionNotifier.Disconnected += OnDisconect;

        if(p2pInfo.Instance.IsHost()){
            p2pInfo.Instance.ConnectionNotifier.Connected += () => Debug.Log($"{p2pConfig.Instance.LastConnectedUsersId} Join");
        }
    }

    void OnDisconect(){
        Debug.Log($"{p2pInfo.Instance.LastDisconnectedUsersId} is Disconnected / {p2pInfo.Instance.ClosedReason}");
    }
}
```