+++
title = "matchState"
weight = 6
+++
## matchState
<small>*Namespace: SynicSugar.MatchMake* <br>
*Class: MatchMakeManager* </small>

public [MatchGUIState](../../MatchGUIState/) matchState

### Description
GUI process in matchmaking.

Can set this value on UnityEditor.


### For process
The Processes to switch button state or flag can be pass as UnityEvent.<br>
In many games, the MatchMake button becomes interactive after a search, and the button cannot be pressed for some time. Then, a user can leave the room after joining the lobby.

| API | description |
|---|---|
| stopAdditionalInput | Stop additional searches or cancellations |
| acceptCancel | Accept cancellation process |

1. Press [start match make] button.
2. Make [start match make] disable not to press multiple times. -> stopAdditionalInput
3. Change [start match make] text to [stop match make]. -> acceptCancel
4. (On Success/Failure) Inactive [start match make]. -> stopAdditionalInput
   
### For Text

| API | description |
|---|---|
| Search | Searching for Lobby and attempts to join |
| Wait | Have Waited for others in (Created/Joined) Lobby |
| Connect | Host's updating lobby to start p2p |
| Success | MatchMaking has finished and ready for p2p |
| Fail | Matchmake fails at some point, (and will return false) |
| Cancel | Leaving Lobby to cancel matchmaking |


example
discriptions.searchLobby = "Searching for an opponent...";<br>
discriptions.waitothers = "Waiting for an opponent...";<br>
discriptions.tryconnect = "Try to connect...";<br>
discriptions.success = "Success MatchMaking";<br>
discriptions.fail = "Fail to match make";<br>
discriptions.trycancel = "Try to Disconnect...";

