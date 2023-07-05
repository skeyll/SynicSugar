+++
title = "playerprefsSaveKey"
weight = 3
+++
## playerprefsSaveKey
<small>*Namespace: SynicSugar.MatchMake* <br>
*Class: MatchMakeManager* </small>

public string playerprefsSaveKey


### Description
The Key to save and load LobbyID of currently belonging to Lobby. 
Valid only when Playerprefs is selected on lobbyIdSaveType.

Can set this value on UnityEditor.
default is eos_lobbyid


```cs
using SynicSugar.MatchMake;

public class MatchmakingConfig : MonoBehaviour {
    public void ChangeConfig(){
        MatchMakeManager.Instance.playerprefsSaveKey = "savekey";
    }
}
```