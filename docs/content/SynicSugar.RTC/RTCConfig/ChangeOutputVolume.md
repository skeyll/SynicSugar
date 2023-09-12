+++
title = "ChangeOutputVolume"
weight = 9
+++
## ChangeOutputVolume
<small>*Namespace: SynicSugar.RTC*</small>

public static void ChangeOutputVolume(float changeRate)


### Description
Change output volume.<br>
0 is mute, 50 is not changed from source value, 100 make volume double.


```cs
using SynicSugar.RTC;
using UnityEngine;
public class MicSetting : MonoBehaviour {
    public void Start(){
        //Make output value double.
        RTCConfig.ChangeOutputVolume(100);
    }
}
```