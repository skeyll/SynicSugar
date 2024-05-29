+++
title = "TimeoutSec"
weight = 1
+++
## TimeoutSec
<small>*Namespace: SynicSugar.MatchMake* <br>
*Class: MatchMakeManager* </small>

public ushort TimeoutSec


### Description
This time is from the start of matchmaking until the the end of matchmaking(= just before preparation for p2p connect). If that time passes before users start p2p, User leave lobby and the matchmaking APIs return false as Timeout. <br>
When we need the more time than 10 minutes for timeout, we can set TimeoutSec directly.


```cs
using SynicSugar.MatchMake;

public class MatchmakingConfig : MonoBehaviour {
    public void ChangeConfig(){
        //5min
        MatchMakeManager.Instance.TimeoutSec = 300;
    }
}
```