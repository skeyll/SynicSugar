#pragma warning disable CS0414 //The field is assigned but its value is never used
using System;
using Cysharp.Threading.Tasks;
using SynicSugar.P2P;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SynicSugar.RTC {
    public class AudioDeviceChangedNotifier {
        public event Action OnDeviceChanged;
        
        public void Register(Action deviceChanged){
            OnDeviceChanged += deviceChanged;
        }
        internal void Clear(){
            OnDeviceChanged = null;
        }
        internal void DeviceChanged(){
            OnDeviceChanged?.Invoke();
        }
        internal async UniTask MonitorGameToUnsubscribe(string sceneName){
            await UniTask.WaitUntil(() => SceneManager.GetActiveScene().name != sceneName);
            RTCConfig.Instance.RemoveNotifyAudioDevicesChanged();
        }
        internal async UniTask MonitorGameToUnsubscribe(GameObject targetObject){
            await UniTask.WaitUntil(() => !targetObject.activeSelf, cancellationToken: targetObject.GetCancellationTokenOnDestroy());
            RTCConfig.Instance.RemoveNotifyAudioDevicesChanged();
        }
    }
    public class ParticipantUpdatedNotifier {
        public event Action OnStartSpeaking;
        public event Action OnStopSpeaking;
        public event Action<UserId> OnStartTargetSpeaking;
        public event Action<UserId> OnStopTargetSpeaking;
        internal UserId TargetId { get; private set; }
        public void Register(Action startSpeaking, Action stopSpeaking){
            OnStartSpeaking += startSpeaking;
            OnStopSpeaking += stopSpeaking;
        }
        public void Register(Action<UserId> startSpeaking, Action<UserId> stopSpeaking){
            OnStartTargetSpeaking += startSpeaking;
            OnStopTargetSpeaking += stopSpeaking;
        }
        internal void Clear(){
            OnStartSpeaking = null;
            OnStopSpeaking = null;
            OnStartTargetSpeaking = null;
            OnStopTargetSpeaking = null;
        }
        internal void StartSpeaking(UserId targetId){
            TargetId = targetId;
            OnStartTargetSpeaking?.Invoke(targetId);
            OnStartSpeaking?.Invoke();
        }
        internal void StopSpeaking(UserId targetId){
            TargetId = targetId;
            OnStopTargetSpeaking?.Invoke(targetId);
            OnStopSpeaking?.Invoke();
        }
    }
}