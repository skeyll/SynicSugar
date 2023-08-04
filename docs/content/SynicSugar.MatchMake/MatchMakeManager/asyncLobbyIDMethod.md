+++
title = "asyncLobbyIDMethod"
weight = 7
+++
## asyncLobbyIDMethod
<small>*Namespace: SynicSugar.MatchMake* <br>
*Class: MatchMakeManager* </small>

LobbyIDMethod asyncLobbyIDMethod


### Description
The Fincs to save and delete LobbyID currently belonging.<br>
We use this for the way to save and delete LobbyID that have return UniTask.<br>
If we don't use **public void Register(Func&lt;UniTask&gt; save, Func&lt;UniTask&gt; delete, bool changeType = true)** to add the events, need switch *[lobbyIdSaveType](../MatchMakeManager/lobbyidsavetype)* to **AsyncCustomMethod**.


Save is called when matchmaking is complete. In other words, Lobby is filled and the matchmaking is closed, then call Save.<br>
Delete is called on the last of ConnectHub *[ExitSession](../../SynicSugar.P2P/ConnectHub/exitsession)* or *[CloseSession](../../SynicSugar.P2P/ConnectHub/closesession)*. If user disconnect the session in any other way, SynicSugar determined that the user needs to be reconnected.<br>
Return result after finish these async process.

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
using Cysharp.Threading.Tasks;

public class MatchMake {
    void SetMatchMakeCondition(){
        //Use api (this switch type automatically)
        MatchMakeManager.Instance.asyncLobbyIDMethod.Register(() => SaveMethod(), () => DeleteMethod());

        //Standard event way and change type
        MatchMakeManager.Instance.lobbyIDMethod.Save += () => SaveMethod();
        MatchMakeManager.Instance.lobbyIDMethod.Delete += () => DeleteMethod();
        MatchMakeManager.Instance.lobbyIdSaveType = MatchMakeManager.RecconectLobbyIdSaveType.AsyncCustomMethod;
    }

    async UniTask SaveMethod(){

    }

    async UniTask DeleteMethod(){

    }
}
```