+++
title = "SetAudioInputDevice"
weight = 5
+++
## SetAudioInputDevice
<small>*Namespace: SynicSugar.RTC*</small>

public static void SetAudioInputDevice(AudioInputDeviceInfo deviceInfo, bool isMute = false)


### Description
Change InputDevice. We can also set whether to mute the input device at the same time.<br>
[AudioInputDeviceInfo](../RTCStruct/audioinputdeviceInfo)


```cs
using System.Collections.Generic;
using SynicSugar.RTC;
using UnityEngine;
public class MicSetting : MonoBehaviour {

    public void Start(){
        List<AudioInputDeviceInfo> inputs = RTCConfig.GetInputDeviceInformation();
        //Mute the device in whole game instead of one session.
        RTCConfig.SetAudioInputDevice(inputs[0], true);
        Debug.Log(inputs[0].DeviceName);
    }
}
```