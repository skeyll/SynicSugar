+++
title = "ConcludeMatchMake"
weight = 23
+++
## ConcludeMatchMake
<small>*Namespace: SynicSugar.MatchMake* <br>
*Class: MatchMakeManager* </small>

public void ConcludeMatchMake()

### Description
Host finishes a matchmaking by hand.After the minimum requirement is met, Host can call this process.<br>
This is valid only when we pass **minLobbyMember** to *[MatchMakingGUIEvents](../MatchMakeManager/matchmakingguievents)*, *[SearchLobby](../MatchMakeManager/searchlobby)*, *[CreateLobby](../MatchMakeManager/createlobby)*.<br>
We can display the GUI Button to invoke this with *[MatchMakingGUIEvents](../MatchMakeManager/matchmakingguievents)*.


```cs
using UnityEngine;
using UnityEngine.UI;
using SynicSugar.MatchMake;

public class MatchMake : MonoBehaviour {
    public Button conclude;
    public void FinishMatchMakingAfterMeetRequiredCondition(){
        MatchMakeManager.Instance.ConcludeMatchMake();
        conclude.gameobject.SetActive(false);
    }
}
```