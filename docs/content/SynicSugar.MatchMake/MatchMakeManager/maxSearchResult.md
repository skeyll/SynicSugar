+++
title = "maxSearchResult"
weight = 0
+++
## maxSerchResult
<small>*Namespace: SynicSugar.MatchMake* <br>
*Class: MatchMakeManager* </small>

pulbic uint maxSearchResult


### Description
The lobby amount of result to retrieve that meet the condition and can join when searching for lobbies. User attempt to join from the results. 

Can set this value on UnityEditor.

default is 5


```cs
using SynicSugar.MatchMake;

public class MatchmakingConfig : MonoBehaviour {
    public void ChangeConfig(){
        MatchMakeManager.Instance.maxSearchResult = 10;
    }
}
```