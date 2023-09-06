#pragma warning disable CS0414 //The field is assigned but its value is never used
using System;
using Cysharp.Threading.Tasks;
using SynicSugar.P2P;
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
    public class ParticipantUpdatedNotifier {
        public event Action StartSpeaking;
        public event Action StopSpeaking;
        public event Action<UserId> StartTargetSpeaking;
        public event Action<UserId> StopTargetSpeaking;
        internal UserId TargetId { get; private set; }
        public void Register(Action startSpeaking, Action stopSpeaking){
            StartSpeaking += startSpeaking;
            StopSpeaking += stopSpeaking;
        }
        public void Register(Action<UserId> startSpeaking, Action<UserId> stopSpeaking){
            StartTargetSpeaking += startSpeaking;
            StopTargetSpeaking += stopSpeaking;
        }
        internal void Clear(){
            StartSpeaking = null;
            StopSpeaking = null;
            StartTargetSpeaking = null;
            StopTargetSpeaking = null;
        }
        internal void OnStartSpeaking(UserId targetId){
            Debug.Log("OnStartSpeaking");
            TargetId = targetId;
            StartTargetSpeaking?.Invoke(targetId);
            StartSpeaking?.Invoke();
        }
        internal void OnStopSpeaking(UserId targetId){
            Debug.Log("OnStopSpeaking");
            TargetId = targetId;
            StopTargetSpeaking?.Invoke(targetId);
            StopSpeaking?.Invoke();
        }
    }
}