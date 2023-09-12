+++
title = "ToggleReceiveingFromTarget"
weight = 9
+++
## ToggleReceiveingFromTarget
<small>*Namespace: SynicSugar.RTC*</small>

public void ToggleReceiveingFromTarget(UserId targetId, bool isEnable)


### Description
Switch Output setting(Enable or Mute) of receiving from target user on this Session.<br>
If targetId is null, effect to all remote users.


```cs
using SynicSugar.RTC;
using UnityEngine;

public class VCSample : MonoBehaviour {
    public void Start(){
        RTCManager.Instance.ToggleReceiveingFromTarget(null, true);
    }
}
```