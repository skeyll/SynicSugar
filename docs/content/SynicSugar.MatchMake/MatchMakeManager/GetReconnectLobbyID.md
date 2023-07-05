+++
title = "GetReconnectLobbyID"
weight = 16
+++
## GetReconnectLobbyID
<small>*Namespace: SynicSugar.MatchMake* <br>
*Class: MatchMakeManager* </small>

public string GetReconnectLobbyID()


### Description
Get LobbyID of the lobby belonging to from Playerprefs. If exists, return LobbyID string. If not, return System.String.Empty.

 Only for **RecconectLobbyIdSaveType.Playerprefs**.


```cs
using SynicSugar.MatchMake;

public class MatchMake : MonoBehaviour {
     void Start(){
        string LobbyID = GetReconnectLobbyID();
    }
}
```