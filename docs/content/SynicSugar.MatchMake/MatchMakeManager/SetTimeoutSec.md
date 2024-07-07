+++
title = "SetTimeoutSec"
weight = 15
+++
## GetCurrentLobbyID
<small>*Namespace: SynicSugar.MatchMake* <br>
*Class: MatchMakeManager* </small>

public void SetTimeoutSec(ushort MatchmakingTimeout = 180, ushort InitialConnectionTimeout = 15)


### Description
Set Timeout sec. Should call this before start matchmake. <br>
/// If use this, need call this before start matchmaking.

#### MatchmakingTimeout
Timeout sec from the start of matchmaking until the the end of matchmaking(= just before preparation for p2p connect).<br>
If nothing is passed, pass the value set in Editor.<br>
Recommend:30-300
#### InitialConnectionTimeout
Timeout sec from the start of preparation for p2p until the the end of the preparetion. <br>
If nothing is passed, pass the value set in Editor.<br>
Recommend:5-20</param>

```cs
using SynicSugar.MatchMake;

public class MatchMake : MonoBehaviour {
     void Start(){
        MatchMakeManager.Instance.SetTimeoutSec(InitialConnectionTimeout: 10);
        //
        //Start Matchmaking
    }
}
```