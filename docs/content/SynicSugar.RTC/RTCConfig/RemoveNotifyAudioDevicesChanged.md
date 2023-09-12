+++
title = "RemoveNotifyAudioDevicesChanged"
weight = 2
+++
## RemoveNotifyAudioDevicesChanged
<small>*Namespace: SynicSugar.RTC*</small>

public void RemoveNotifyAudioDevicesChanged()


### Description
Remove Notify of AudioDevicesChanged in manual.<br>
*[AddNotifyAudioDevicesChanged](../RTCConfig/addnotifyaudiodeviceschanged)*


```cs
using SynicSugar.RTC;
using UnityEngine;
public class MicSetting : MonoBehaviour {
    public void MoveToGame(){
        //Remove notify
        RTCConfig.Instance.RemoveNotifyAudioDevicesChanged();
        //Change scene
    }
    void RefreshShownValue(){}
}
```