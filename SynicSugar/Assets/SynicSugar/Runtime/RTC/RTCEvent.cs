#pragma warning disable CS0414 //The field is assigned but its value is never used
using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SynicSugar.RTC {
    public class AudioDeviceChangedNotifier {
        public event Action DeviceChanged;
        
        public void Register(Action deviceChanged){
            DeviceChanged += deviceChanged;
        }
        internal void Clear(){
            DeviceChanged = null;
        }
        internal void OnDeviceChanged(){
            DeviceChanged?.Invoke();
        }
        internal async UniTask MonitorGameToUnsubscribe(string sceneName){
            await UniTask.WaitUntil(() => SceneManager.GetActiveScene().name != sceneName);
            RTCConfig.Instance.RemoveNotifyAudioDevicesChanged();
            Clear();
        }
        internal async UniTask MonitorGameToUnsubscribe(GameObject targetObject){
            await UniTask.WaitUntil(() => !targetObject.activeSelf);
            RTCConfig.Instance.RemoveNotifyAudioDevicesChanged();
            Clear();
        }
    }
}