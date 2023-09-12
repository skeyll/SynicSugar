+++
title = "IsTargetMutedOnLocal"
weight = 4
+++
## IsTargetMutedOnLocal
<small>*Namespace: SynicSugar.RTC*</small>

public bool IsTargetMutedOnLocal(UserId target) 


### Description
Whether the target is muted in this local<br>


```cs
using SynicSugar.P2P;
using SynicSugar.RTC;
using UnityEngine;

public class VCSample : MonoBehaviour {
    public void GetMuteState(UserId target){
        string state = RTCManager.Instance.IsTargetMutedOnLocal(target) ? "Muted" : " NotMuted";
        Debug.Log($"{target} is {state}");
    }
}
```