+++
title = "SetAudioOutputDevice"
weight = 8
+++
## SetAudioOutputDevice
<small>*Namespace: SynicSugar.RTC*</small>

public static void SetAudioOutputDevice(AudioOutputDeviceInfo deviceInfo)


### Description
Change OutputDevice.<br>
*[AudioOutputDeviceInfo](../RTCStruct/audiooutputdeviceinfo)*, *[ChangeOutputVolume](../RTCConfig/changeoutputvolume)*


```cs
using System.Collections.Generic;
using SynicSugar.RTC;
using UnityEngine;
public class MicSetting : MonoBehaviour {
    public void Start(){
        List<AudioOutputDeviceInfo> outputs = RTCConfig.GetOutputDeviceInformation();
        RTCConfig.SetAudioOutputDevice(inputs[0]);
    }
}
```