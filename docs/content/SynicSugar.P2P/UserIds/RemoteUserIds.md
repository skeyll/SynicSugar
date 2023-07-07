+++
title = "RemoteUserIds"
weight = 1
+++
## RemoteUserIds
<small>*Namespace: SynicSugar.P2P* <br>
*Class: UserIds* </small>

public List<UserId> RemoteUserIds;


### Description
Remote UserIDs list.<br>
This List contains of only current active user in the session. If a user falls from connect, it's moved to *[LeftUsers](../UserIds/leftusers)*.


```cs
using SynicSugar.P2P;

public class p2pSample {
    void UserIDsSample(){
        UserID id = p2pConfig.Instance.userIds.RemoteUserIds[0];
    }
}
```