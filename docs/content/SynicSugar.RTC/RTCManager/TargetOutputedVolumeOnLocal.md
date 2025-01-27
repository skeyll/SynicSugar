+++
title = "TargetOutputedVolumeOnLocal"
weight = 3
+++
## TargetOutputedVolumeOnLocal
<small>*Namespace: SynicSugar.RTC*</small>

public float TargetOutputedVolumeOnLocal(UserId target)<br>
public bool TryGetTargetOutputedVolumeOnLocal(UserId target, out float volume)


### Description
Outputed volume on this local. We don't know target local setting.<br>
0-100. 50 means source volume.


```cs
using SynicSugar.P2P;
using SynicSugar.RTC;
using UnityEngine;

public class VCSample : MonoBehaviour {
    public void GetTargetVolume(UserId target){
        Debug.Log(RTCManager.Instance.TargetOutputedVolumeOnLocal(target));
    }
}
```