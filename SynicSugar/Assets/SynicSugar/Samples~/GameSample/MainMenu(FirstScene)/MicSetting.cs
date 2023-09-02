using System.Collections.Generic;
using SynicSugar.RTC;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
namespace SynicSugar.Samples{
    //This setting just for chat scene (having VC).
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
            //To remove for manual
            // RTCConfig.Instance.AddNotifyAudioDevicesChanged(() => RefreshShownValue());
            //To remove notify by scene.
            RTCConfig.Instance.AddNotifyAudioDevicesChanged(SceneManager.GetActiveScene().name, () => RefreshShownValue());
        }
        public void RefreshShownValue(){
            inputDevices.options.Clear();
            outputDevices.options.Clear();
            foreach(var device in inputs){
                string defaultText = device.DefaultDevice ? "(Default)" : System.String.Empty;
                string name = $"{device.DeviceName}{defaultText}";
                
                inputDevices.options.Add(new Dropdown.OptionData { text = name });
                if(device.DefaultDevice){
                    inputDevices.value = inputDevices.options.Count;
                }
            }

            foreach(var device in outputs){
                string defaultText = device.DefaultDevice ? "(Default)" : System.String.Empty;
                string name = $"{device.DeviceName}{defaultText}";
                
                outputDevices.options.Add(new Dropdown.OptionData { text = name });
                if(device.DefaultDevice){
                    outputDevices.value = outputDevices.options.Count;
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
}

