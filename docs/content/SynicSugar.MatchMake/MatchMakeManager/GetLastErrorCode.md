+++
title = "GetLastErrorCode"
weight = 26
+++
## GetLastErrorCode
<small>*Namespace: SynicSugar.MatchMake* <br>
*Class: MatchMakeManager* </small>

public SynicSugar.Result GetLastErrorCode()


### Description
Get Last ERROR Result code. None means init state. <br>
After call matchmaking apis, this value becomes **Result.None**.<br>
-3 - -1 is unique code of SynicSugar. Others is same with Epic's Result code.


```cs
using SynicSugar.MatchMake;
using UnityEngine;

public class MatchmakingManager : MonoBehaviour {
    public void StartMatchmaking(){
        bool isSuccess = await MatchMakeManager.Instance.SearchAndCreateLobby(matchConditions.GetLobbyCondition(32), minLobbyMember: 5);

        if(!isSuccess){
            Debug.LogFormat("Matchmaking failed. {0}", MatchMakeManager.Instance.GetLastErrorCode());
            return;
        }
    }
}
```