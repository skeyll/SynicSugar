+++
title = "hostsTimeoutSec"
weight = 1
+++
## hostsTimeoutSec
<small>*Namespace: SynicSugar.MatchMake* <br>
*Class: MatchMakeManager* </small>

public int hostsTimeoutSec


### Description
The seconds of Host user to leave lobby when waiting for other users. This start counting after the Host creates the lobby.

When this time has gone without other user joining, matchmaking functions returns false, and the matchmaking fails. Give Lobby Host to other user.

Can set this value on UnityEditor.
default is 180


```cs
using SynicSugar.MatchMake;

public class MatchmakingConfig : MonoBehaviour {
    public void ChangeConfig(){
        MatchMakeManager.Instance.hostsTimeoutSec = 180;
    }
}
```