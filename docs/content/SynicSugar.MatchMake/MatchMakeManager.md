+++
title = "MatchMakeManager"
weight = 0
+++

## MatchMakeManager
<small>*Namespace: SynicSugar.MatchMake*</small>

This is used like **MatchMakeManager.Instance.XXX()**.


### Description
This script is Mono's Singleton attached to ConnenctManager. Drop this **ConnenctManager** into the scene from *Packages/SynicSugar/Runtime/Prefabs/ConnectManager*. <br>
ConnectManager has **DontDestroy**, so ConnectManager will not be destroyed by scene transitions. This is used for re-connection, and also needed for p2p scene. <br>

If this is no longer needed, we call *[CancelCurrentMatchMake](../MatchMakeManager/cancelcurrentmatchmake)*, *[ExitSession](../../SynicSugar.P2P/ConnectHub/exitsession)* and *[CloseSession](../../SynicSugar.P2P/ConnectHub/exitsession)*.



### Properity
| API | description |
|---|---|
| [maxSearchResult](../MatchMakeManager/maxsearchresult)  | The amount of search results |
| [hostsTimeoutSec](../MatchMakeManager/hoststimeoutsec) | Timeout seconds for Host to leave not filled lobby |
| [lobbyIdSaveType](../MatchMakeManager/lobbyidsavetype) | The way to return to the disconnected lobby |
| [playerprefsSaveKey](../MatchMakeManager/playerprefssavekey) | the key to save LobbyID |
| [customSaveLobbyID](../MatchMakeManager/customsavelobbyid) | UnityEvent to save LobbyID |
| [customDeleteLobbyID](../MatchMakeManager/customdeletelobbyid) | UnityEvent to delete LobbyID |
| [lobbyIDMethod](../MatchMakeManager/lobbyidmethod) | Actions to recconect Lobby |
| [asyncLobbyIDMethod](../MatchMakeManager/asynclobbyidmethod) | Func&lt;UniTask&gt; to recconect Lobby |
| [matchState](../MatchMakeManager/matchstate) | Text and Button state on GUI in matchmaking |


### Function 
| API | description |
|---|---|
| [SetGUIState](../MatchMakeManager/setguistate) | Change *MatchState* from script |
| [SearchAndCreateLobby](../MatchMakeManager/searchandcreatelobby) | Search lobby and, if can't join, create lobby |
| [SearchLobby](../MatchMakeManager/searchlobby) | Search lobby and join it as Guest |
| [CreateLobby](../MatchMakeManager/createlobby) | Create lobby as Host and wait for Guest |
| [ReconnectLobby](../MatchMakeManager/reconnectlobby) | Join the Lobby with saved LobbyID |
| [CancelCurrentMatchMake](../MatchMakeManager/cancelcurrentmatchmake) | Stop the current matchmaking |
| [GetCurrentLobbyID](../MatchMakeManager/getcurrentlobbyid) | Get LobbyID that a user participating |
| [GetReconnectLobbyID](../MatchMakeManager/getreconnectlobbyid) | Get LobbyID by Playerprefs |
| [GenerateLobbyObject](../MatchMakeManager/generatelobbyobject) | Generate a lobby object for conditions |


```cs
using SynicSugar.MatchMake;

public class MatchMake {
    void SetMatchMakeCondition(){
        string LobbyID = MatchMakeManager.Instance.GetReconnectLobbyID();
    }
}
```