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
| [maxSearchResult](../MatchMakeManager/maxsearchresult)  | The amount of search results |
| [matchTimeoutSec](../MatchMakeManager/matchtimeoutsec)| Timeout sec of Lobby Host user |
| [AllowUserReconnect](../MatchMakeManager/allowuserreconnect)| If true, can re-connect to the disconnected match |
| [matchState](../MatchMakeManager/matchstate)| Text and Button state on GUI in Matching |


### Function 
| API | description |
|---|---|
| [SetGUIState](../MatchMakeManager/setguistate) | Change *MatchState* from script |
| [StartMatchMake](../MatchMakeManager/startmtchmake) |  Search lobby, then if can't join create lobby |
| [ReconnectParticipatingLobby](../MatchMakeManager/reconnectparticipatinglobby/) | Join the Lobby with saved LobbyID |
| [DestroyHostingLobby](../MatchMakeManager/loginwithdeviceid) | Destroy hostted lobby |