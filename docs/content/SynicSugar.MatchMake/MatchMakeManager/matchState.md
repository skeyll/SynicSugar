+++
title = "matchState"
weight = 3
+++
## matchState
public class MatchGUIState

### Description
GUI process in matchmaking.


### For process
The Processes to switch button state or flag can be pass as UnityEvent.
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
| Search | User searches for Lobby and attempts to join |
| Wait | User wait for others in lobby |
| Connect | Host update lobby for p2p  |
| Success | MatchMake is complete and ready for p2p |
| Fail | Matchmake fails at some point |
| Cancel | Leave joining lobby by manually |

example
discriptions.searchLobby = "Searching for an opponent...";
discriptions.waitothers = "Waiting for an opponent...";
discriptions.tryconnect = "Try to connect...";
discriptions.success = "Success MatchMaking";
discriptions.fail = "Fail to match make";
discriptions.trycancel = "Try to Disconnect...";

