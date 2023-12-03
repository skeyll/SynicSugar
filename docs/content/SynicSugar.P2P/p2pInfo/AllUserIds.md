+++
title = "AllUserIds"
weight = 0
+++
## AllUserIds
<small>*Namespace: SynicSugar.P2P* <br>
*Class: p2pInfo* </small>

public List<UserId> AllUserIds;


### Description
UserIDs of this whole this session.<br>
This Id is assigned by EOS.<br>
The order is the same in lobby order, so all locals have the same order of this.<br>
When user will leave the lobby in manual, the id is removed from this List.<br>
LocalUserId + CurrentRemoteUserIds + DisconnectedUserIds

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