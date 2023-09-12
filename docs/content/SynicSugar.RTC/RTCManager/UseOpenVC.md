+++
title = "UseOpenVC"
weight = 1
+++
## UseOpenVC
<small>*Namespace: SynicSugar.RTC*</small>

public bool UseOpenVC;


### Description
Switch OpenVC and PushToTalk.<br>
**This is valid only before matching.** If we want to switch that after matching, call ToggleLocalUserSending() by ourself.

```cs
using SynicSugar.RTC;
using UnityEngine;

public class VCSample : MonoBehaviour {
    void Start(){
        //Push to Talk
        RTCManager.Instance.UseOpenVC = false;
    }
}
```