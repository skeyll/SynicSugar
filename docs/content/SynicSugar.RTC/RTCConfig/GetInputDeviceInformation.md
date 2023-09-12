+++
title = "GetInputDeviceInformation"
weight = 3
+++
## GetInputDeviceInformation
<small>*Namespace: SynicSugar.RTC*</small>

public static List&lt;AudioInputDeviceInfo&gt; GetInputDeviceInformation()


### Description
Get Device List to input vc.<br>
[AudioInputDeviceInfo](../RTCStruct/audioinputdeviceInfo)


```cs
using System.Collections.Generic;
using SynicSugar.RTC;
using UnityEngine;
public class MicSetting : MonoBehaviour {

    public void Start(){
        List<AudioInputDeviceInfo> inputs = RTCConfig.GetInputDeviceInformation();
        List<AudioOutputDeviceInfo> outputs = RTCConfig.GetOutputDeviceInformation();
        
        Debug.Log(inputs[0].DeviceName);
    }
}
```