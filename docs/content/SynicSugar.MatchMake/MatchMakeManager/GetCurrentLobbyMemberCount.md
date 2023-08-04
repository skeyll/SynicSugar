+++
title = "GetCurrentLobbyMemberCount"
weight = 17
+++
## GetCurrentLobbyMemberCount
<small>*Namespace: SynicSugar.MatchMake* <br>
*Class: MatchMakeManager* </small>

public int GetCurrentLobbyMemberCount()


### Description
Get the current member count in Lobby.


```cs
using SynicSugar.MatchMake;
using UnityEngine;
using UnityEngine.UI;

public class MatchMakeSample : MonoBehaviour {
    [SerializeField] Text memberCount;
    
    public void UpdateMemberCount(){
        memberCount.text = $"{MatchMakeManager.Instance.GetCurrentLobbyMemberCount()} / {MatchMakeManager.Instance.GetMaxLobbyMemberCount()}";
    }
}
```