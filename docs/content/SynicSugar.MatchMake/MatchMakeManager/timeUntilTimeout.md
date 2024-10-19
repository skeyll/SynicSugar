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
This time counts only for the time a local user is waiting for opponents in matchmaking. After closing lobby, this time is not used and p2pSetupTimeoutSec is used.<br>


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