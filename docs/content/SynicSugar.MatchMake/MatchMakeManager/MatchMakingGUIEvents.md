+++
title = "MatchMakingGUIEvents"
weight = 8
+++
## MatchMakingGUIEvents
<small>*Namespace: SynicSugar.MatchMake* <br>
*Class: MatchMakeManager* </small>

public MatchMakingGUIEvents MatchMakingGUIEvents

### Description
To manage GUI in matchmaking. Invoke these on each phase.<br>

Can set some value on UnityEditor.


### Properity
| API | description |
|---|---|
| canKick | True after meet required member counts |
| stateText | GUIText to display matchmaking state |

### Event
| API | description |
|---|---|
| DisableStart | Invoked when start matchmaking |
| EnableCancelKick | Invoked when join and create lobby |
| EnableHostConclude |Invoked when filled in the minimum number of members |
| DisableHostConclude | Invoked when members leave and condition is no longer met |
| DisableCancelKickConclude | Invoked when complete or cancel matchmaking |
| OnLobbyMemberCountChanged<UserId, bool> | Invoked when the number of participants are changed |

   
### For Text
| API | description |
|---|---|
| StartMatchmaking | Searching or creating lobby |
| WaitForOpponents | Waiting for others in Lobby |
| FinishMatchmaking | After finish matchmaking, start to exchange info for p2p |
| ReadyForConnection | MatchMaking has completed and ready for p2p |
| TryToCancel | Leaving or destroying lobby |
| StartReconnection | Try to join with SavedLobbyID |


```cs
using SynicSugar.MatchMake;

public class MatchMakeConfig {
    public enum Langugage{
        EN, JA
    }
    public static MatchMakingGUIEvents SetMatchingText(Langugage langugage){
        MatchMakingGUIEvents descriptions = new();
        switch(langugage){
            case Langugage.JA:
                descriptions.StartMatchmaking = "マッチングを検索中";
                descriptions.WaitForOpponents = "対戦相手を探しています";
                descriptions.FinishMatchmaking = "接続準備中・・・";
                descriptions.ReadyForConnection = "接続準備完了";
                descriptions.TryToCancel = "マッチングキャンセル中・・・";
                descriptions.StartReconnection = "再接続しています";
            break;
            default:
                descriptions.StartMatchmaking = "Searching for lobby.";
                descriptions.WaitForOpponents = "Waiting for opponents...";
                descriptions.FinishMatchmaking = "Preparetion for Connection";
                descriptions.ReadyForConnection = "Finish Matchmaking";
                descriptions.TryToCancel = "Try to Disconnect...";
                descriptions.StartReconnection = "Try to reconnection...";
            break;
        }
        return descriptions;
    }
}
```