+++
title = "ChangeInputMuteStatus"
weight = 6
+++
## ChangeInputMuteStatus
<small>*Namespace: SynicSugar.RTC*</small>

public static void ChangeInputMuteStatus(bool isMute)


### Description
Change input volume status.<br>


```cs
using SynicSugar.RTC;
using UnityEngine;
public class MicSetting : MonoBehaviour {
    public void Start(){
        //Mute the device in whole game instead of one session.
        RTCConfig.ChangeInputMuteStatus(true);
    }
}
```