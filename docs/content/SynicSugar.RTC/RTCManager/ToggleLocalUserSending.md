+++
title = "ToggleLocalUserSending"
weight = 8
+++
## ToggleLocalUserSending
<small>*Namespace: SynicSugar.RTC*</small>

public void ToggleLocalUserSending(bool isEnable)


### Description
Switch Input setting of Local user sending on this Session.<br>
This is Low API. We should [StartVoiceSending()](../RTCManager/startvoicesending) and [StopVoiceSending()](../RTCManager/stopvoicesending) instead of this. 


```cs
using SynicSugar.RTC;
using UnityEngine;

public class VCSample : MonoBehaviour {
    public void Start(){
        RTCManager.Instance.ToggleLocalUserSending(true);
    }
}
```