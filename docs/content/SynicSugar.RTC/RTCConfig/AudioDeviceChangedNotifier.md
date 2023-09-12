+++
title = "AudioDeviceChangedNotifier"
weight = 0
+++
## AddNotifyAudioDevicesChanged
<small>*Namespace: SynicSugar.RTC*</small>

public AudioDeviceChangedNotifier AudioDeviceChangedNotifier


### Description
Invoke an event on AudioDeviceChanged. 

### Event
| API | description |
|---|---|
| OnDeviceChanged | Invoke on AudioDeviceChanged |


### Function
| API | description |
|---|---|
| Register | Set events |


```cs
using SynicSugar.RTC;
using UnityEngine;
public class MicSetting : MonoBehaviour {
    void Start(){
        //Add event
        RTCConfig.Instance.AudioDeviceChangedNotifier.OnDeviceChanged += RefreshShownValue;
        //Remove Event
        RTCConfig.Instance.AudioDeviceChangedNotifier.OnDeviceChanged -= RefreshShownValue;
    }

    public void RefreshShownValue(){
    }
}
```