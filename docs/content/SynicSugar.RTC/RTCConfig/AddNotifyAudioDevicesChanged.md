+++
title = "AddNotifyAudioDevicesChanged"
weight = 1
+++
## AddNotifyAudioDevicesChanged
<small>*Namespace: SynicSugar.RTC*</small>

public void AddNotifyAudioDevicesChanged(Action OnDeviceChangedAction = null)<br>
public void AddNotifyAudioDevicesChanged(string currentSceneName, Action OnDeviceChangedAction = null)<br>
public void AddNotifyAudioDevicesChanged(GameObject MoniterTargetObject, Action OnDeviceChangedAction = null)


### Description
Register to receive notifications when an audio device is added or removed to the system.<br>
Action is triggered when the device is added or removed.<br>
The way to cancel this notifier is in MANUAL by calling RemoteNotifyAudioDevicesChanged(), or in AUTO when the current scene changes or when the object becomes inactive.<br>

This notify is mainly used when the user switches the device configuration on the setting screen (to display new devices on dropdown). <br>
In normal, Library automatically switches removed device to the available one. We don't need to moniter devices in game. *[EOS Document](https://dev.epicgames.com/docs/ja/api-ref/functions/eos-rtc-audio-add-notify-audio-devices-changed)*

```cs
using System.Collections.Generic;
using SynicSugar.RTC;
using UnityEngine;
using UnityEngine.UI;
public class MicSetting : MonoBehaviour {
    [SerializeField] Dropdown inputDevices, outputDevices;
    [SerializeField] GameObject settingCanvas;
    List<AudioInputDeviceInfo> inputs;
    List<AudioOutputDeviceInfo> outputs;
    void Start(){
        inputs = RTCConfig.GetInputDeviceInformation();
        outputs = RTCConfig.GetOutputDeviceInformation();
        //To remove notify when canvas is disactivated.
        RTCConfig.Instance.AddNotifyAudioDevicesChanged(settingCanvas, () => RefreshShownValue());
    }
    public void RefreshShownValue(){
        inputDevices.options.Clear();
        outputDevices.options.Clear();
        for(int i = 0; i < inputs.Count; i++){
            string defaultText = inputs[i].DefaultDevice ? "(Default)" : System.String.Empty;
            string name = $"{defaultText} {inputs[i].DeviceName}";
            
            inputDevices.options.Add(new Dropdown.OptionData { text = name });
            if(inputs[i].DefaultDevice){
                inputDevices.value = i;
            }
        }

        for(int i = 0; i < outputs.Count; i++){
            string defaultText = outputs[i].DefaultDevice ? "(Default)" : System.String.Empty;
            string name = $"{defaultText} {outputs[i].DeviceName}";
            
            outputDevices.options.Add(new Dropdown.OptionData { text = name });
            if(outputs[i].DefaultDevice){
                outputDevices.value = i;
            }
        }
        inputDevices.RefreshShownValue();
        outputDevices.RefreshShownValue();
    }
}
```