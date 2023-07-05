+++
title = "GetCurrentLobbyID"
weight = 15
+++
## GetCurrentLobbyID
<small>*Namespace: SynicSugar.MatchMake* <br>
*Class: MatchMakeManager* </small>

public string GetCurrentLobbyID()


### Description
Get ID of the current lobby that a user's participating.

*I'll use this in future for in-game process.


```cs
using SynicSugar.MatchMake;

public class MatchMake : MonoBehaviour {
     void Start(){
        string LobbyID = GetCurrentLobbyID();
    }
}
```