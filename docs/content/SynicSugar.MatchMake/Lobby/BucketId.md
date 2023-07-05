+++
title = "BucketId"
weight = 1
+++
## BucketId
<small>*Namespace: SynicSugar.MatchMake* <br>
*Class: Lobby* </small>

public string BucketId 


### Description
Bucket ID associated with the lobby.


```cs
using SynicSugar.MatchMake;

public class LobbyCondition : MonoBehaviour {
    void SetLobbyConditions(){
        Lobby lobby = new Lobby();
        lobby.BucketId = "EU:RANK";
    }
}
```