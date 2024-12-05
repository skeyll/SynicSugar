+++
title = "timeUntilTimeout"
weight = 1
+++
## timeUntilTimeout
<small>*Namespace: SynicSugar.MatchMake* <br>
*Class: MatchMakeManager* </small>

public float timeUntilTimeout { get; private set; }


### Description
Sec until stopping the process to wait for opponents.<br>
This value is typically 0. <br>
It is set to the same value as `timeoutSec` just before matchmaking starts. At that time, `IsMatchmaking` and `isLooking` also become true.<br>
While `isLooking` is true, meaning from the start of matchmaking until a peer is found and P2P preparation begins, this value continues to count down.<br>
Once `isLooking` becomes false and p2p preparation is complete, `IsMatchmaking` is set to false, this value is reset to 0, and the Matchmaking API's result is returned.


```cs
using UnityEngine;
using UnityEngine.UI;
using SynicSugar.MatchMake;

public class MatchmakingConfig : MonoBehaviour {
    public Text time;
    public void Update(){
        //Display time until timeout on GUI.
        time.text = ((int)MatchMakeManager.Instance.timeUntilTimeout).ToString();
    }
}
```