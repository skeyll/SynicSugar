+++
title = "TimeoutSec"
weight = 1
+++
## TimeoutSec
<small>*Namespace: SynicSugar.MatchMake* <br>
*Class: MatchMakeManager* </small>

public int TimeoutSec


### Description
The seconds of user to exit lobby when waiting for other users. This count start after call Api on Matchmake.<br>
When this time has gone without lobby is filled, matchmaking apis returns false, and the matchmaking fails. If the user is Host, give the role to other user as Host, then leave the Lobby.<br>

Can set this value on UnityEditor.<br>
If we want to set a bigger number than the range on the Editor, set it via script.<br>
default is 180


```cs
using SynicSugar.MatchMake;

public class MatchmakingConfig : MonoBehaviour {
    public void ChangeConfig(){
        MatchMakeManager.Instance.hostsTimeoutSec = 180;
    }
}
```