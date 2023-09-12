+++
title = "RTCConfig"
weight = 0
+++

## RTCConfig
<small>*Namespace: SynicSugar.RTC*</small>


### Description
RTC config for device instead of one session.


### Properity
| API | description |
|---|---|
| [AudioDeviceChangedNotifier](../RTCConfig/audiodevicechangednotifier) | Events for OnDeviceChanged |


### Public Function
| API | description |
|---|---|
| [AddNotifyAudioDevicesChanged](../RTCConfig/addnotifyaudiodeviceschanged) | Notify to get device changed on System |
| [RemoveNotifyAudioDevicesChanged](../RTCConfig/removenotifyaudiodeviceschanged) | Remove NotifyAudioDevicesChanged notify |


### Static Function
| API | description |
|---|---|
| [GetInputDeviceInformation](../RTCConfig/getinputdeviceinformation) | Get Devices to input vc |
| [SetAudioInputDevice](../RTCConfig/setaudioinputdevice) | Change InputDevice |
| [ChangeInputMuteStatus](../RTCConfig/changeinputmutestatus) | Change input volume to mute or not |
| [GetOutputDeviceInformation](../RTCConfig/getoutputdeviceinformation) | Get Devices to output vc |
| [SetAudioOutputDevice](../RTCConfig/setaudiooutputdevice) | Change OutputDevice |
| [ChangeOutputVolume](../RTCConfig/changeoutputvolume) | Change output volume of device |


```cs
using System.Collections.Generic;
using SynicSugar.RTC;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
//This settings for local device.
public class MicSetting : MonoBehaviour {
    [SerializeField] Dropdown inputDevices, outputDevices;
    List<AudioInputDeviceInfo> inputs;
    List<AudioOutputDeviceInfo> outputs;
    void Start(){
        inputs = RTCConfig.GetInputDeviceInformation();
        outputs = RTCConfig.GetOutputDeviceInformation();
        
        inputDevices.onValueChanged.AddListener(i => OnInputSelected(i));
        outputDevices.onValueChanged.AddListener(i => OnOutputSelected(i));

        RefreshShownValue();
        //To remove notify by scene.
        RTCConfig.Instance.AddNotifyAudioDevicesChanged(SceneManager.GetActiveScene().name, () => RefreshShownValue());
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
    void OnInputSelected(int index){
        RTCConfig.SetAudioInputDevice(inputs[index]);
    }
    void OnOutputSelected(int index){
        RTCConfig.SetAudioOutputDevice(outputs[index]);
    }
}

```