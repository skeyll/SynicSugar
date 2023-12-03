+++
title = "MatchMakeManager"
weight = 0
+++

## MatchMakeManager
<small>*Namespace: SynicSugar.MatchMake*</small>

This is used like **MatchMakeManager.Instance.XXX()**.


### Description
This script is Mono's Singleton attached to NetworkManager.  To generate NetworkManager, right-click on the Hierarchy and click SynicSugar/NetworkManager.<br>
NetworkManager has **DontDestroy**, so NetworkManager will not be destroyed by scene transitions. This is used for re-connection, and also needed for p2p scene. <br>

If this is no longer needed, we call *[CancelCurrentMatchMake](../MatchMakeManager/cancelcurrentmatchmake)*, *[ExitSession](../../SynicSugar.P2P/ConnectHub/exitsession)* and *[CloseSession](../../SynicSugar.P2P/ConnectHub/exitsession)*.



### Properity
| API | description |
|---|---|
| [maxSearchResult](../MatchMakeManager/maxsearchresult)  | The amount of search results |
| [TimeoutSec](../MatchMakeManager/timeoutsec) | Timeout seconds for user to exit no-filled lobby |
| [lobbyIdSaveType](../MatchMakeManager/lobbyidsavetype) | The way to return to the disconnected lobby |
| [playerprefsSaveKey](../MatchMakeManager/playerprefssavekey) | The key to save LobbyID |
| [customSaveLobbyID](../MatchMakeManager/customsavelobbyid) | UnityEvent to save LobbyID |
| [customDeleteLobbyID](../MatchMakeManager/customdeletelobbyid) | UnityEvent to delete LobbyID |
| [lobbyIDMethod](../../SynicSugar.MatchMake/lobbyidmethod) | Actions to recconect Lobby |
| [asyncLobbyIDMethod](../../SynicSugar.MatchMake/asynclobbyidmethod) | Func&lt;UniTask&gt; to recconect Lobby |
| [MatchMakingGUIEvents](../../SynicSugar.MatchMake/matchmakingguievents) | To manage GUI in matchmaking |
| [MemberUpdatedNotifier](../MatchMakeManager/memberupdatednotifier) | Notify when a user attributes is updated |
| [LastResultCode](../MatchMakeManager/lastresultcode) | ResultCode if get error |
| [isHost](../MatchMakeManager/ishost) | Whether this local user is the owner of current Lobby |


### Function 
| API | description |
|---|---|
| [SearchAndCreateLobby](../MatchMakeManager/searchandcreatelobby) | Search lobby and, if can't join, create lobby |
| [SearchLobby](../MatchMakeManager/searchlobby) | Search lobby and join it as Guest |
| [CreateLobby](../MatchMakeManager/createlobby) | Create lobby as Host and wait for Guest |
| [ConcludeMatchMake](../MatchMakeManager/concludematchmake) | Host finishes a matchmaking by hand |
| [ReconnectLobby](../MatchMakeManager/reconnectlobby) | Join the Lobby with saved LobbyID |
| [CancelCurrentMatchMake](../MatchMakeManager/cancelcurrentmatchmake) | Stop the current matchmaking |
| [CloseCurrentMatchMake](../MatchMakeManager/closecurrentmatchmake) | Host destroys and Guest leaves lobby to stop the matchmaking |
| [KickTargetFromLobby](../MatchMakeManager/kicktargetfromlobby) | (Host) kicks target from Lobby |
| [GetLastErrorCode](../MatchMakeManager/getlasterrorcode) | Get Last ERROR Result code |
| [GetCurrentLobbyID](../MatchMakeManager/getcurrentlobbyid) | Get LobbyID that a user participating |
| [GetReconnectLobbyID](../MatchMakeManager/getreconnectlobbyid) | Get LobbyID by Playerprefs |
| [GetCurrentLobbyMemberCount](../MatchMakeManager/getcurrentlobbymembercount) | Get the current member count in Lobby |
| [GetMaxLobbyMemberCount](../MatchMakeManager/getmaxlobbymembercount) | Get the current lobby's member limit |
| [GenerateLobbyObject](../MatchMakeManager/generatelobbyobject) | Generate a lobby object for conditions |
| [GetTargetAttributeData](../MatchMakeManager/gettargetattributedata) | Get attribute(s) of a member |
| [GetLastErrorCode](../MatchMakeManager/getlasterrorcode) | Get Last ERROR Result code of Matchmaking |
| [isLocalUserId](../MatchMakeManager/islocaluserid) | Whether the argument is the id of local user or not |


```cs
using SynicSugar.MatchMake;

public class MatchMake {
    void SetMatchMakeCondition(){
        string LobbyID = MatchMakeManager.Instance.GetReconnectLobbyID();
    }
}
```