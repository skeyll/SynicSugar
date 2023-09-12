+++
title = "GetOutputDeviceInformation"
weight = 7
+++
## GetOutputDeviceInformation
<small>*Namespace: SynicSugar.RTC*</small>

public static List&lt;AudioOutputDeviceInfo&gt; GetOutputDeviceInformation()


### Description
Get Devices to output vc.<br>
*[AudioOutputDeviceInfo](../RTCStruct/audiooutputdeviceinfo)*


```cs
using System.Collections.Generic;
using SynicSugar.RTC;
using UnityEngine;
public class MicSetting : MonoBehaviour {
    public void Start(){
        List<AudioInputDeviceInfo> inputs = RTCConfig.GetInputDeviceInformation();
        List<AudioOutputDeviceInfo> outputs = RTCConfig.GetOutputDeviceInformation();
        
        Debug.Log(outputs[0].DeviceName);
    }
}
```