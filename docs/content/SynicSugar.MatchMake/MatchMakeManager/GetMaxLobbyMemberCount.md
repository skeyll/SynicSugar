+++
title = "GetMaxLobbyMemberCount"
weight = 18
+++
## GetMaxLobbyMemberCount
<small>*Namespace: SynicSugar.MatchMake* <br>
*Class: MatchMakeManager* </small>

public int GetMaxLobbyMemberCount()


### Description
Get the current lobby's member limit.


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