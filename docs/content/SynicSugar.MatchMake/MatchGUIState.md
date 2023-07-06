+++
title = "MatchGUIState"
weight = 3
+++

## MatchGUIState
<small>*Namespace: SynicSugar.MatchMake*</small>


### Description
GUI process in matchmaking. Set this in MatchMakeManager.<br>
This is used to switch button process amd display matchmaking progress.<br>
Check [matchState](../../MatchMakeManager/matchstate).


### Constructor

| API | description |
|---|---|
| MatchGUIState()  | Instantiate new MatchGUIState class |
| MatchGUIState(Text uiText) | Instantiate new one with uiText to display a match progress |


### Properity
| API | description |
|---|---|
| state  | Unity UI Text to display a matchmaking progress |
| stopAdditionalInput | Stop user additional input after starting matchmaking |
| acceptCancel | Accept cancel after having created or joined Lobby, if the matchmaking hasn't finished yet. |
| searchLobby | Searching for Lobby and attempts to join |
| waitothers  | Have Waited for others in (Created/Joined) Lobby |
| tryconnect | Host's updating lobby to start p2p |
| success  | MatchMaking has finished and ready for p2p |
| fail | Matchmake fails at some point, (and will return false) |
| trycancel  | Leaving Lobby to cancel matchmaking |

<br>

```cs
using UnityEngin.UI;
using SynicSugar.MatchMake;

public class LobbyCondition : MonoBehaviour {
    [SerializeField] Text stateText;
    [SerializeField] string language;
    void SetLobbyConditions(){
        MatchGUIState matchState = new MatchGUIState(stateText);

        matchState.stopAdditionalInput.AddListener(StopAdditionalInput);
        matchState.acceptCancel.AddListener(() => ActivateCancelButton(true));

        switch(language){
            case "EN":
                matchState.searchLobby = "Searching for an opponent...";
                matchState.waitothers = "Waiting for an opponent...";
                matchState.tryconnect = "Try to connect...";
                matchState.success = "Success MatchMaking";
                matchState.fail = "Fail to match make";
                matchState.trycancel = "Try to Disconnect...";
            break;
            case "JP":
                matchState.searchLobby = "対戦相手検索中・・・";
                matchState.waitothers = "対戦相手を探しています";
                matchState.tryconnect = "接続中・・・";
                matchState.success = "成功！";
                matchState.fail = "失敗！";
                matchState.trycancel = "マッチングキャンセル中・・・";
            break;
            case "CH":
            //
            break;
        }

        //

        MatchMakeManager.Instance.SetGUIState(matchState);
    }
    public void StopAdditionalInput(){
        MATCH_START_BUTTON_OBJECT.SetActive(false);
    }
    public void ActivateCancelButton(bool isActivate){
        MATCH_CHANCEL_BUTTON_OBJECT.SetActive(isActivate);
    }
}
```