+++
title = "MatchMakeManager"
weight = 0
+++

## MatchMakeManager

### Description
This script is Mono's Singleton attached to EOSp2pManager. Can get this from Packages/SynicSugar/Runtime/Prefabs/EOSp2pManager.
EOSp2pManager has **DontDestroy**, so EOSp2pManager will not be destroyed by scene transitions. This is used for re-connection, and also needed for p2p scene.


### Properity
| API | description |
|---|---|
| [maxSerchResult](./maxSerchResult)  | The amount of search results |
| [matchTimeoutSec](./matchTimeoutSec)| Timeout sec of Lobby Host user |
| [AllowUserReconnect](./AllowUserReconnect)| If true, can re-connect to the disconnected match |
| [matchState](./matchState)| Text and Button state on GUI in Matching |


### Function 
| API | description |
|---|---|
| [SetGUIState](./SetGUIState) | Change *MatchState* from script |
| [StartMatchMake](./StartMatchMake) |  Search lobby, then if can't join create lobby |
| [ReconnectParticipatingLobby](./ReconnectParticipatingLobby) | Join the Lobby with saved LobbyID |
| [DestroyHostingLobby](./LoginWithDeviceID) | Destroy hostted lobby |