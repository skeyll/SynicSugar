+++
title = "MatchMakeManager"
weight = 0
+++

## MatchMakeManager

### Description
Singleton.


### Properity
| API | description |
|---|---|
|  [maxSerchResult](./maxSerchResult.md)  | The amount of search results |
| [matchTimeoutSec](./matchTimeoutSec)| Timeout sec of Lobby Host user |
| [AllowUserReconnect](./AllowUserReconnect)| If true, can re-connect to the disconnected match |
| [matchState](./matchState)| Text and Button state on GUI in Matching |


### Function 
| API | description |
|---|---|
|  [SetGUIState](./SetGUIState.md)  | Change *MatchState* from script |
|  [StartMatchMake](./StartMatchMake.md)  |  Search lobby, then if can't join create lobby  |
|  [ReconnectParticipatingLobby](./ReconnectParticipatingLobby.md)  | Join the Lobby with saved LobbyID  |
|  [DestroyHostingLobby](./LoginWithDeviceID.md)  | Destroy hostted lobby  |