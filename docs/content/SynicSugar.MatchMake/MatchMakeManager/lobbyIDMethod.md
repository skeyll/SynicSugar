+++
title = "lobbyIDMethod"
weight = 6
+++
## lobbyIDMethod
<small>*Namespace: SynicSugar.MatchMake* <br>
*Class: MatchMakeManager* </small>

LobbyIDMethod lobbyIDMethod


### Description
The Actions to save and delete LobbyID currently belonging.<br>
We use this for the way to save and delete LobbyID that have no return value.<br>
If we don't use Register(Action save, Action delete, bool changeType = true) to add the events, need switch *[lobbyIdSaveType](../MatchMakeManager/lobbyidsavetype)* to **CustomMethod**.

If set *[customSaveLobbyID](../MatchMakeManager/customsavelobbyid)* on editor, this is registered on Awake automatically.

Save is called when matchmaking is complete. In other words, Lobby is filled and the matchmaking is closed, then call Save.<br>
Delete is called on the last of ConnectHub *[ExitSession](../../SynicSugar.P2P/ConnectHub/exitsession)* or *[CloseSession](../../SynicSugar.P2P/ConnectHub/exitsession)*. If user disconnect the session in any other way, SynicSugar determined that the user needs to be reconnected.


### Properity
| API | description |
|---|---|
| Save | Event to save LobbyID to reconnect Lobby. |
| Delete | Event to save LobbyID to reconnect Lobby. |


### Properity
| API | description |
|---|---|
| Register | Set events and switch lobbyIdSaveType |


```cs
using SynicSugar.MatchMake;

public class MatchMake {
    void SetMatchMakeCondition(){
        //Use api (this switch type automatically)
        MatchMakeManager.Instance.lobbyIDMethod.Register(() => SaveMethod(), () => DeleteMethod());

        //Standard event way and change type
        MatchMakeManager.Instance.lobbyIDMethod.Save += () => SaveMethod();
        MatchMakeManager.Instance.lobbyIDMethod.Delete += () => DeleteMethod();
        MatchMakeManager.Instance.lobbyIdSaveType = MatchMakeManager.RecconectLobbyIdSaveType.CustomMethod;
    }

    void SaveMethod(){

    }

    void DeleteMethod(){

    }
}
```