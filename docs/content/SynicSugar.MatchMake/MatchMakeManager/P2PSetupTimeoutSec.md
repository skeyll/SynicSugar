+++
title = "p2pSetupTimeoutSec"
weight = 1
+++
## p2pSetupTimeoutSec
<small>*Namespace: SynicSugar.MatchMake* <br>
*Class: MatchMakeManager* </small>

public ushort p2pSetupTimeoutSec


### Description
This time is from the start of preparation for p2p until the the end of the preparetion.<br>
If the timeout occurs before the matchmaking APIs return Success, they will return Result.Canceled as a Failure response.<br>
When we need the more time than 1 minutes for timeout, we can set TimeoutSec directly.<br>
If call SetTimeoutSec after matchmaking has started could cause bugs, so set this in the Editor or call SetTimeoutSec before matchmaking.


```cs
using SynicSugar.MatchMake;

public class MatchmakingConfig : MonoBehaviour {
    public void ChangeConfig(){
        MatchMakeManager.Instance.p2pSetupTimeoutSec = 20;
    }
}
```