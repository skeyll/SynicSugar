+++
title = "isHost"
weight = 22
+++
## isHost
<small>*Namespace: SynicSugar.MatchMake* <br>
*Class: MatchMakeManager* </small>

public bool isHost


### Description
Whether this local user is the owner of current Lobby. **Only valid during matchmaking.**


```cs
using Cysharp.Threading.Tasks;
using SynicSugar.MatchMake;
using SynicSugar.P2P;
using UnityEngine;
using UnityEngine.UI;

public class MatchmakingManager : MonoBehaviour {
    public Button Kick;
    //For MemberUpdatedNotifier
    public void SetData(UserId id){
        if(MatchMakeManager.Instance.isHost && !MatchMakeManager.Instance.isLocalUserId(id)){
            Kick.gameObject.SetActive(true);
            Kick.onClick.AddListener(() => MatchMakeManager.Instance.KickTargetFromLobby(id).Forget());
        }
    }
}
```