+++
title = "IsTargetHardMuted"
weight = 5
+++
## IsTargetHardMuted
<small>*Namespace: SynicSugar.RTC*</small>

public bool IsTargetHardMuted(UserId target) 


### Description
Whether the target is hard-muted in this session.<br>
This is valid only for Lobby Host. Guests don't know user's hard-muted.


```cs
using SynicSugar.P2P;
using SynicSugar.RTC;
using UnityEngine;

public class VCSample : MonoBehaviour {
    public void GetMuteState(UserId target){
        string state = RTCManager.Instance.IsTargetHardMuted(target) ? "Muted" : " NotMuted";
        Debug.Log($"{target} is {state}");
    }
}
```