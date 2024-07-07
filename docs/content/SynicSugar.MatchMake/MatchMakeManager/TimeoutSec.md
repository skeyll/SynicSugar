+++
title = "TimeoutSec"
weight = 1
+++
## TimeoutSec
<small>*Namespace: SynicSugar.MatchMake* <br>
*Class: MatchMakeManager* </small>

public ushort TimeoutSec


### Description
This time is from the start of matchmaking until the the end of matchmaking(= just before preparation for p2p connect).<br>
If that time passes before users start p2p setup, the matchmaking APIs return false as Timeout.<br>
When we need the more time than 10 minutes for timeout, we can set TimeoutSec directly.<br>
If call SetTimeoutSec after matchmaking has started could cause bugs, so set this in the Editor or call SetTimeoutSec before matchmaking.


```cs
using SynicSugar.MatchMake;

public class MatchmakingConfig : MonoBehaviour {
    public void ChangeConfig(){
        //5min
        MatchMakeManager.Instance.TimeoutSec = 300;
    }
}
```