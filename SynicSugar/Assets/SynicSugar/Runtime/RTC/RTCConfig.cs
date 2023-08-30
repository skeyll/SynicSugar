using System.Collections;
using System.Collections.Generic;
using PlayEveryWare.EpicOnlineServices;
using Epic.OnlineServices;
using Epic.OnlineServices.RTC;
using Epic.OnlineServices.RTCAudio;
using UnityEngine;
using ResultE = Epic.OnlineServices.Result;

namespace SynicSugar.RTC {
    public class RTCConfig {
        /// <summary>
        /// Get Device List to Input.
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
        ///
        /// </summary>
        /// <param name="deviceInfo"></param>
        // NOTE: This will be DEPRECATED in future SDK.
        // https://dev.epicgames.com/docs/ja/api-ref/functions/eos-rtc-audio-set-audio-input-settings
        public static void SetAudioInputDevice(AudioInputDeviceInfo deviceInfo){
            RTCInterface rtcInterface = EOSManager.Instance.GetEOSRTCInterface();
            RTCAudioInterface audioInterface = rtcInterface.GetAudioInterface();
            var settingOptions = new SetAudioInputSettingsOptions(){
                LocalUserId = EOSManager.Instance.GetProductUserId(),
                DeviceId = deviceInfo.DeviceId
            };
            ResultE result = audioInterface.SetAudioInputSettings(ref settingOptions);

            if(result != ResultE.Success){
                Debug.LogErrorFormat("SetAudioInputDevice: failed. {0}", result);
            }
        #if SYNICSUGAR_LOG
            Debug.Log("SetAudioInputDevice: set a device as Input Device.");
        #endif
        }
        public static void ChangeAudioMuteStatus(bool isMute){
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
    }
    public class AudioInputDeviceInfo{
        public Utf8String DeviceId { get; private set; }
        public Utf8String DeviceName { get; private set; }
        public bool DefaultDevice { get; private set; }
        internal AudioInputDeviceInfo(Epic.OnlineServices.RTCAudio.AudioInputDeviceInfo? info){
            DeviceId = info?.DeviceId;
            DeviceName = info?.DeviceName;
            DefaultDevice = info?.DefaultDevice ?? false;
        }
    }
}
