+++
title = "UpdateReceiveingVolumeFromTarget"
weight = 10
+++
## UpdateReceiveingVolumeFromTarget
<small>*Namespace: SynicSugar.RTC*</small>

public void UpdateReceiveingVolumeFromTarget(UserId targetId, float volume)


### Description
Change the output volume on this session.<br>
If targetId is null, effect to all remote users.<br>
Range 0.0 - 100. 50 means that the audio volume is not modified its source value. 100 makes volume double.


```cs
using SynicSugar.RTC;
using UnityEngine;

public class VCSample : MonoBehaviour {
    public void Start(){
        //Mute
        RTCManager.Instance.UpdateReceiveingVolumeFromTarget(null, 0f);
        //Double
        RTCManager.Instance.UpdateReceiveingVolumeFromTarget(null, 100f);
    }
}
```