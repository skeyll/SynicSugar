+++
title = "lobbyIdSaveType"
weight = 2
+++
## lobbyIdSaveType
<small>*Namespace: SynicSugar.MatchMake* <br>
*Class: MatchMakeManager* </small>

public RecconectLobbyIdSaveType lobbyIdSaveType<br>
public enum RecconectLobbyIdSaveType {
    NoReconnection, Playerprefs, CustomMethod, AsyncCustomMethod
}


### Description
The way to save LobbyID to recconect to disconnect lobby.
This "disconnect" means that the app down and the user left the lobby unexpectedly. Just p2p disconnections are automatically reconnected. If all users fall, the lobby will be destroyed, so can't reconnect.

Can set this value on UnityEditor.

default is Playerprefs.


```cs
using SynicSugar.MatchMake;

public class MatchmakingConfig : MonoBehaviour {
    public void ChangeConfig(){
        MatchMakeManager.Instance.lobbyIdSaveType = MatchMakeManager.RecconectLobbyIdSaveType.CustomMethod;
    }
}
```