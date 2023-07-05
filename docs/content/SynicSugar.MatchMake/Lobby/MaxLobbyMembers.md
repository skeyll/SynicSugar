+++
title = "MaxLobbyMembers"
weight = 0
+++
## MaxLobbyMembers
<small>*Namespace: SynicSugar.MatchMake* <br>
*Class: Lobby* </small>

public uint MaxLobbyMembers 

### Description
Capacity of Lobby that Host's creating.


```cs
using SynicSugar.MatchMake;

public class LobbyCondition : MonoBehaviour {
    void SetLobbyConditions(){
        Lobby lobby = new Lobby();
        lobby.MaxLobbyMembers = 10;
    }
}
```