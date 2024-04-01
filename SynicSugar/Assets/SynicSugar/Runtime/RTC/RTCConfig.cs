using System.Collections.Generic;
using PlayEveryWare.EpicOnlineServices;
using Epic.OnlineServices.RTC;
using Epic.OnlineServices.RTCAudio;
using UnityEngine;
using ResultE = Epic.OnlineServices.Result;
using System;
using Cysharp.Threading.Tasks;

namespace SynicSugar.RTC {
    /// <summary>
    /// Setting on local.
    /// </summary>
    public class RTCConfig {
        private static readonly RTCConfig instance = new RTCConfig();
        private RTCConfig(){}
        public static RTCConfig Instance { get { return instance; } }
        ulong AudioDevicesChangedId;
        /// <summary>
        /// Events for OnDeviceChanged.
        /// </summary>
        public AudioDeviceChangedNotifier AudioDeviceChangedNotifier = new();
        /// <summary>
        /// Register to receive notifications when an audio device is added or removed to the system.<br />
        /// Action is triggered when the device is added or removed.<br />
        /// The way to cancel this notifier is in MANUAL by calling RemoteNotifyAudioDevicesChanged(), or in AUTO when the current scene changes or when the object becomes inactive.<br />
        /// This needs MANUAL remove.
        /// </summary>
        /// <param name="OnDeviceChangedAction">Notification is called when the list on OS of Input and Output devices is changes, and this event is invoked. <br />
        /// This notify is mainly used when the user switches the device configuration on the setting screen (to display new devices on dropdown). <br />
        /// In normal, Library automatically switches removed device to the available one.<br />
        /// https://dev.epicgames.com/docs/ja/api-ref/functions/eos-rtc-audio-add-notify-audio-devices-changed
        /// </param>
        public void AddNotifyAudioDevicesChanged(Action OnDeviceChangedAction = null){
            if(AudioDevicesChangedId == 0){
                RTCInterface rtcInterface = EOSManager.Instance.GetEOSRTCInterface();
                RTCAudioInterface audioInterface = rtcInterface.GetAudioInterface();
                if(OnDeviceChangedAction != null){
                    AudioDeviceChangedNotifier.OnDeviceChanged += OnDeviceChangedAction;
                }

                var changedOptions = new AddNotifyAudioDevicesChangedOptions();
                AudioDevicesChangedId = audioInterface.AddNotifyAudioDevicesChanged(ref changedOptions, null, OnAudioDevicesChanged);
                if(AudioDevicesChangedId == 0){
                    Debug.LogError("AddNotifyAudioDevicesChanged: is failed");
                }
            }
        }
        /// <summary>
        /// Register to receive notifications when an audio device is added or removed to the system.<br />
        /// Action is triggered when the device is added or removed.<br />
        /// The way to cancel this notifier is in MANUAL by calling RemoteNotifyAudioDevicesChanged(), or in AUTO when the current scene changes or when the object becomes inactive.<br />
        /// This notify is removed automatically when scene is changed.
        /// </summary>
        /// <param name="currentSceneName">Notify is automatically removes when scene is moved from this scene.</param>
        /// <param name="OnDeviceChangedAction">Notification is called when the list on OS of Input and Output devices is changes, and this event is invoked. <br />
        /// This notify is mainly used when the user switches the device configuration on the setting screen (to display new devices on dropdown). <br />
        /// In normal, Library automatically switches removed device to the available one.<br />
        /// https://dev.epicgames.com/docs/ja/api-ref/functions/eos-rtc-audio-add-notify-audio-devices-changed
        /// </param>
        public void AddNotifyAudioDevicesChanged(string currentSceneName, Action OnDeviceChangedAction = null){
            if(AudioDevicesChangedId == 0){
                RTCInterface rtcInterface = EOSManager.Instance.GetEOSRTCInterface();
                RTCAudioInterface audioInterface = rtcInterface.GetAudioInterface();
                if(OnDeviceChangedAction != null){
                    AudioDeviceChangedNotifier.OnDeviceChanged += OnDeviceChangedAction;
                }
                AudioDeviceChangedNotifier.MonitorGameToUnsubscribe(currentSceneName).Forget();

                var changedOptions = new AddNotifyAudioDevicesChangedOptions();
                AudioDevicesChangedId = audioInterface.AddNotifyAudioDevicesChanged(ref changedOptions, null, OnAudioDevicesChanged);
                if(AudioDevicesChangedId == 0){
                    Debug.LogError("AddNotifyAudioDevicesChanged: is failed");
                }
            }
        }
        /// <summary>
        /// Register to receive notifications when an audio device is added or removed to the system.<br />
        /// Action is triggered when the device is added or removed.<br />
        /// The way to cancel this notifier is in MANUAL by calling RemoteNotifyAudioDevicesChanged(), or in AUTO when the current scene changes or when the object becomes inactive.<br />
        /// This notify is removed automatically when scene is changed.
        /// </summary>
        /// <param name="MoniterTargetObject">Notify is automatically removes when the target is deleted or dis-activate.</param>
        /// <param name="OnDeviceChangedAction">Notification is called when the list on OS of Input and Output devices is changes, and this event is invoked. <br />
        /// This notify is mainly used when the user switches the device configuration on the setting screen (to display new devices on dropdown). <br />
        /// In normal, Library automatically switches removed device to the available one.<br />
        /// https://dev.epicgames.com/docs/ja/api-ref/functions/eos-rtc-audio-add-notify-audio-devices-changed
        /// </param>
        public void AddNotifyAudioDevicesChanged(GameObject MoniterTargetObject, Action OnDeviceChangedAction = null){
            if(AudioDevicesChangedId == 0){
                RTCInterface rtcInterface = EOSManager.Instance.GetEOSRTCInterface();
                RTCAudioInterface audioInterface = rtcInterface.GetAudioInterface();
                if(OnDeviceChangedAction != null){
                    AudioDeviceChangedNotifier.OnDeviceChanged += OnDeviceChangedAction;
                }
                AudioDeviceChangedNotifier.MonitorGameToUnsubscribe(MoniterTargetObject).Forget();
                
                var changedOptions = new AddNotifyAudioDevicesChangedOptions();
                AudioDevicesChangedId = audioInterface.AddNotifyAudioDevicesChanged(ref changedOptions, null, OnAudioDevicesChanged);
                if(AudioDevicesChangedId == 0){
                    Debug.LogError("AddNotifyAudioDevicesChanged: is failed");
                }
            }
        }
        void OnAudioDevicesChanged(ref AudioDevicesChangedCallbackInfo info){
        #if SYNICSUGAR_LOG
            Debug.Log("OnAudioDevicesChanged: audio device is changed.");
        #endif
            AudioDeviceChangedNotifier.DeviceChanged();
        }
        /// <summary>
        /// Remove AudioDevicesChanged in manual.
        /// </summary>
        public void RemoveNotifyAudioDevicesChanged(){
            if(AudioDevicesChangedId != 0){
                RTCInterface rtcInterface = EOSManager.Instance.GetEOSRTCInterface();
                RTCAudioInterface audioInterface = rtcInterface.GetAudioInterface();
                audioInterface.RemoveNotifyAudioDevicesChanged(AudioDevicesChangedId);
                AudioDeviceChangedNotifier.Clear();
                AudioDevicesChangedId = 0;
            }
        }
        /// <summary>
        /// Get Device List to input vc.
        /// </summary>
        /// <returns></returns>
        public static List<AudioInputDeviceInfo> GetInputDeviceInformation(){
            RTCInterface rtcInterface = EOSManager.Instance.GetEOSRTCInterface();
            RTCAudioInterface audioInterface = rtcInterface.GetAudioInterface();
            var countOptions = new GetAudioInputDevicesCountOptions(){};
            uint deviceCount = audioInterface.GetAudioInputDevicesCount(ref countOptions);
            
            List<AudioInputDeviceInfo> devicesInfo = new();
            for (uint i = 0; i < deviceCount; i++){
                uint index = i;
                var deviceByIndexOptions = new GetAudioInputDeviceByIndexOptions(){
                    DeviceInfoIndex = index
                };

                var info = audioInterface.GetAudioInputDeviceByIndex(ref deviceByIndexOptions);
                devicesInfo.Add(new AudioInputDeviceInfo(info));
            }
            return devicesInfo;
        }
        /// <summary>
        /// Change InputDevice.
        /// </summary>
        /// <param name="deviceInfo">AudioInputDeviceInfo from the List to be got by GetInputDeviceInformation().</param>
        /// <param name="isMute"></param>
        // NOTE: This will be DEPRECATED in future SDK.
        // https://dev.epicgames.com/docs/ja/api-ref/functions/eos-rtc-audio-set-audio-input-settings
        public static void SetAudioInputDevice(AudioInputDeviceInfo deviceInfo, bool isMute = false){
            RTCInterface rtcInterface = EOSManager.Instance.GetEOSRTCInterface();
            RTCAudioInterface audioInterface = rtcInterface.GetAudioInterface();
            var settingOptions = new SetAudioInputSettingsOptions(){
                LocalUserId = EOSManager.Instance.GetProductUserId(),
                DeviceId = deviceInfo.DeviceId,
                Volume = isMute ? 0 : 1
            };
            ResultE result = audioInterface.SetAudioInputSettings(ref settingOptions);

            if(result != ResultE.Success){
                Debug.LogErrorFormat("SetAudioInputDevice: failed. {0}", result);
            }
        #if SYNICSUGAR_LOG
            Debug.Log("SetAudioInputDevice: set a device as Input Device.");
        #endif
        }
        /// <summary>
        /// Change volume status.
        /// </summary>
        /// <param name="isMute"></param>
        public static void ChangeInputMuteStatus(bool isMute){
            RTCInterface rtcInterface = EOSManager.Instance.GetEOSRTCInterface();
            RTCAudioInterface audioInterface = rtcInterface.GetAudioInterface();
            var settingOptions = new SetAudioInputSettingsOptions(){
                LocalUserId = EOSManager.Instance.GetProductUserId(),
                Volume = isMute ? 0 : 1
            };
            ResultE result = audioInterface.SetAudioInputSettings(ref settingOptions);

            if(result != ResultE.Success){
                Debug.LogErrorFormat("ChangeAudioMuteStatus: failed. {0}", result);
            }
        #if SYNICSUGAR_LOG
            Debug.Log("ChangeAudioMuteStatus: can change audio volume.");
        #endif
        }
        /// <summary>
        /// Get Device List to output vc.
        /// </summary>
        /// <returns></returns>
        public static List<AudioOutputDeviceInfo> GetOutputDeviceInformation(){
            RTCInterface rtcInterface = EOSManager.Instance.GetEOSRTCInterface();
            RTCAudioInterface audioInterface = rtcInterface.GetAudioInterface();
            var countOptions = new GetAudioOutputDevicesCountOptions(){};
            uint deviceCount = audioInterface.GetAudioOutputDevicesCount(ref countOptions);
            
            List<AudioOutputDeviceInfo> devicesInfo = new();
            for (uint i = 0; i < deviceCount; i++){
                uint index = i;
                var deviceByIndexOptions = new GetAudioOutputDeviceByIndexOptions(){
                    DeviceInfoIndex = index
                };

                var info = audioInterface.GetAudioOutputDeviceByIndex(ref deviceByIndexOptions);
                devicesInfo.Add(new AudioOutputDeviceInfo(info));
            }
            return devicesInfo;
        }
        
        /// <summary>
        /// Change OutputDevice.
        /// </summary>
        /// <param name="deviceInfo">AudioOutputDeviceInfo from the List to be got by GetOutputtDeviceInformation().</param>
        // NOTE: This will be DEPRECATED in future SDK.
        public static void SetAudioOutputDevice(AudioOutputDeviceInfo deviceInfo){
            RTCInterface rtcInterface = EOSManager.Instance.GetEOSRTCInterface();
            RTCAudioInterface audioInterface = rtcInterface.GetAudioInterface();
            var settingOptions = new SetAudioOutputSettingsOptions(){
                LocalUserId = EOSManager.Instance.GetProductUserId(),
                DeviceId = deviceInfo.DeviceId,
            };
            ResultE result = audioInterface.SetAudioOutputSettings(ref settingOptions);

            if(result != ResultE.Success){
                Debug.LogErrorFormat("SetAudioOutputDevice: failed. {0}", result);
            }
        #if SYNICSUGAR_LOG
            Debug.Log("SetAudioOutputDevice: set a device as Output Device.");
        #endif
        }
        /// <summary>
        /// Change volume status.
        /// </summary>
        /// <param name="changeRate">0 is mute, 50 is not changed, 100 make volume double</param>
        public static void ChangeOutputVolume(float changeRate){
            RTCInterface rtcInterface = EOSManager.Instance.GetEOSRTCInterface();
            RTCAudioInterface audioInterface = rtcInterface.GetAudioInterface();
            var settingOptions = new SetAudioOutputSettingsOptions(){
                LocalUserId = EOSManager.Instance.GetProductUserId(),
                Volume = changeRate
            };
            ResultE result = audioInterface.SetAudioOutputSettings(ref settingOptions);

            if(result != ResultE.Success){
                Debug.LogErrorFormat("ChangeOutputVolume: failed. {0}", result);
            }
        #if SYNICSUGAR_LOG
            Debug.Log("ChangeOutputVolume: can change audio volume.");
        #endif
        }
    }
}
