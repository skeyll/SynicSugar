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
}

