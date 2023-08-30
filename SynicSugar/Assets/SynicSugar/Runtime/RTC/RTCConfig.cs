using System.Collections.Generic;
using PlayEveryWare.EpicOnlineServices;
using Epic.OnlineServices.RTC;
using Epic.OnlineServices.RTCAudio;
using UnityEngine;
using ResultE = Epic.OnlineServices.Result;

namespace SynicSugar.RTC {
    /// <summary>
    /// Setting on local.
    /// </summary>
    public class RTCConfig {
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
        /// <param name="isMute"></param>
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
