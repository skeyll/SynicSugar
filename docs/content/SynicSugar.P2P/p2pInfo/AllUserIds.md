+++
title = "AllUserIds"
weight = 0
+++
## AllUserIds
<small>*Namespace: SynicSugar.P2P* <br>
*Class: p2pInfo* </small>

public List<UserId> AllUserIds;


### Description
UserIDs of this whole session. The Id is assigned by EOS.<br>
The order is the same in lobby order, so all locals have the same order of this. Even if the local user is reconnecter, they has the list in same order because the host will sent the list.<br>
Even if user will leave the lobby in manual, this list remains the same. When we can know JUST current user ids, can use CurrentAllUserIds. This list contains LocalUserId + CurrentRemoteUserIds + DisconnectedUserIds. (exclude of LeftUserIds)<br> If we need current connected user ids, can use CurrentConnectedUserIds. It has LocalUserId + CurrentRemoteUserIds. (exclude of DisconnectedUserIds and LeftUserIds)

```cs
using SynicSugar.P2P;
using UnityEngine;
[NetworkPlayer]
public class p2pSample : MonoBehaviour {
    public bool isLocalPlayer;

    void Start(){
        foreach(var id in p2pInfo.Instance.AllUserIds){
            //Check data of all user
        }
    }
}
```