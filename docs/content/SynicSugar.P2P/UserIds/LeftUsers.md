+++
title = "LeftUsers"
weight = 1
+++
## RemoteUserIds
<small>*Namespace: SynicSugar.P2P* <br>
*Class: UserIds* </small>

public List<UserId> LeftUsers;


### Description
Remote UserIDs list.<br>
This List contains of disconnected user in the session. If a user returns to the Lobby, the User's ID is moved to *[RemoteUserIds](../UserIds/remoteuserids)*.


```cs
using SynicSugar.P2P;

public class p2pSample {
    void UserIDsSample(){
        UserID id = p2pConfig.Instance.userIds.LeftUsers[0];
    }
}
```