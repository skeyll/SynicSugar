+++
title = "UserIds"
weight = 2
+++

## UserIds
<small>*Namespace: SynicSugar.P2P*</small>


### Description
This is used Lobby in p2pConfig.


### Constructor
| API | description |
|---|---|
| UserIds() | Generate UserIDsObjects for LobbyObject |


### Properity
| API | description |
|---|---|
| [LocalUserId](../UserIds/localuserid) | Local UserID |
| [RemoteUserIds](../UserIds/remoteuserids) | RemoteUserIDs list |
| [LeftUsers](../UserIds/leftusers) | Disconnected RemoteUserIDs List |


### Function 
| API | description |
|---|---|
| IsHost | If localUser is Host, return true. |


```cs
using SynicSugar.P2P;

public class p2pSample {
    void UserIDsSample(){
        if(p2pConfig.Instance.userIds.IsHost()){
            //This player is Lobby's Host
        }
    }
}
```