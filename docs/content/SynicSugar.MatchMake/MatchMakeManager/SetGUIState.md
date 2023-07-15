+++
title = "SetGUIState"
weight = 9
+++
## SetGUIState
<small>*Namespace: SynicSugar.MatchMake* <br>
*Class: MatchMakeManager* </small>

public void SetGUIState(MatchGUIState state)

### Description 
Change *MatchState* from script for localize. Can set [MatchState](../matchState) on Editor.

```cs
using SynicSugar.MatchMake;

public class MatchMake : MonoBehaviour {
    void Start(){
        MatchGUIState descriptions = new();
        descriptions.searchLobby = "Searching for an opponent...";
        descriptions.waitothers = "Waiting for an opponent...";
        //...
        MatchMakeManager.Instance.SetGUIState(descriptions);
    }
}
```