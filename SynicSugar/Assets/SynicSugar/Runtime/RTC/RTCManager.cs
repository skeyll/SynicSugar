using PlayEveryWare.EpicOnlineServices;
using Epic.OnlineServices;
using Epic.OnlineServices.Lobby;
using Epic.OnlineServices.RTC;
using Epic.OnlineServices.RTCAudio;
using UnityEngine;
using SynicSugar.P2P;
using SynicSugar.MatchMake;
using System.Threading;
using Cysharp.Threading.Tasks;
using ResultE = Epic.OnlineServices.Result;

#if !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif

namespace SynicSugar.RTC {
    public class RTCManager : MonoBehaviour {
        private RTCManager(){}
        public static RTCManager Instance { get; private set; }
        void Awake() {
            if( Instance != null ) {
                Destroy( this );
                return;
            }
            Instance = this;
        }
        void OnDestroy() {
            if( Instance == this ) {
                ParticipantUpdatedNotifier.Clear();

                Instance = null;
            }
        }
        // RTCRoom CurrentRoom = new();
        internal Lobby CurrentLobby { get { return MatchMakeManager.Instance.eosLobby.CurrentLobby; }}
        RTCInterface rtcInterface;
        RTCAudioInterface audioInterface;
        CancellationTokenSource pttToken;
        ulong ParticipantStatusId, ParticipantUpdatedId;
        public ParticipantUpdatedNotifier ParticipantUpdatedNotifier = new();
        /// <summary>
        /// This is valid only before matching. If we want to switch OpenVC and PTT after matching, call ToggleLocalUserSending() ourself.
        /// </summary>
        [Header("If true, all audio is transmitted. If false, push to take.")]
        public bool UseOpenVC;
#if ENABLE_LEGACY_INPUT_MANAGER
        public KeyCode KeyToPushToTalk = KeyCode.Space;
#else
        public UnityEngine.InputSystem.Key KeyToPushToTalk = UnityEngine.InputSystem.Key.Space;
#endif
    #region Notify
        /// <summary>
        /// Register event to get VC status, then start VC.<br />
        /// Must call this to use after Created ot Join Lobby.
        /// </summary>
        internal void AddNotifyParticipantUpdated(){
            if(!CurrentLobby.bEnableRTCRoom){
                return;
            }
            if(System.String.IsNullOrEmpty(CurrentLobby.RTCRoomName)){
                Debug.LogError("AddNotifyParticipantUpdatedOptions: This user does't have not find RTC room.");
                return;
            }
            
            //Notify to get user talking status
            if(ParticipantUpdatedId == 0){
                AddNotifyParticipantUpdatedOptions addNotifyParticipantUpdatedOptions = new AddNotifyParticipantUpdatedOptions(){
                    LocalUserId = EOSManager.Instance.GetProductUserId(),
                    RoomName = CurrentLobby.RTCRoomName
                };
                ParticipantUpdatedId = audioInterface.AddNotifyParticipantUpdated(ref addNotifyParticipantUpdatedOptions, null, OnParticipantUpdated);
                if(ParticipantUpdatedId == 0){
                    Debug.LogError("AddNotifyParticipantUpdated: can not be regisered.");
                    return;
                }
            }
        }
        /// <summary>
        /// Call this close or leave lobby.
        /// </summary>
        internal void RemoveRTCEvents(){
            if(!CurrentLobby.bEnableRTCRoom){
                return;
            }
            if(ParticipantStatusId != 0){
                rtcInterface.RemoveNotifyParticipantStatusChanged(ParticipantStatusId);
            }
            if(ParticipantUpdatedId != 0){
                rtcInterface.GetAudioInterface().RemoveNotifyParticipantUpdated(ParticipantUpdatedId);
            }

            CurrentLobby.RTCRoomName = System.String.Empty;
            CurrentLobby.hasConnectedRTCRoom = false;
        }
        /// <summary>
        /// When using RTC, call OnComplete of Create and Join.
        /// </summary>
        internal void AddNotifyParticipantStatusChanged(){      
            if(!CurrentLobby.bEnableRTCRoom){
                return;
            }
            
            rtcInterface = EOSManager.Instance.GetEOSRTCInterface();
            audioInterface = rtcInterface.GetAudioInterface();

            //Validation of RTC room
            LobbyInterface lobbyInterface = EOSManager.Instance.GetEOSLobbyInterface();

            IsRTCRoomConnectedOptions isConnectedOptions = new IsRTCRoomConnectedOptions(){
                LobbyId = CurrentLobby.LobbyId,
                LocalUserId = EOSManager.Instance.GetProductUserId()
            };
            ResultE result = lobbyInterface.IsRTCRoomConnected(ref isConnectedOptions, out bool isConnected);
            if (result != ResultE.Success){
                Debug.LogFormat("AddNotifyParticipantStatusChanged: This user is not participating in the RTC room.: {0}", result);
                return;
            }

            MatchMakeManager.Instance.eosLobby.CurrentLobby.hasConnectedRTCRoom = isConnected;
            MatchMakeManager.Instance.eosLobby.CurrentLobby.RTCRoomName = GetRTCRoomName();

            //Check audio devices
            var audioOptions = new GetAudioInputDevicesCountOptions();
            audioInterface.GetAudioInputDevicesCount(ref audioOptions);

            var audioOutputOptions = new GetAudioOutputDevicesCountOptions();
            audioInterface.GetAudioOutputDevicesCount(ref audioOutputOptions);
            
            if(ParticipantStatusId == 0){
                // Notify to get a user's joining and leaving
                AddNotifyParticipantStatusChangedOptions StatusChangedOptions = new AddNotifyParticipantStatusChangedOptions(){
                    LocalUserId = EOSManager.Instance.GetProductUserId(),
                    RoomName = CurrentLobby.RTCRoomName
                };
                ParticipantStatusId = rtcInterface.AddNotifyParticipantStatusChanged(ref StatusChangedOptions, null, OnParticipantStatusChanged);

                if(ParticipantStatusId == 0){
                    Debug.LogError("AddNotifyParticipantStatusChanged: RTCRoomParticipantUpdate can not be regisered.");
                }
            }
            string GetRTCRoomName(){
                GetRTCRoomNameOptions options = new GetRTCRoomNameOptions(){
                    LobbyId = CurrentLobby.LobbyId,
                    LocalUserId = EOSManager.Instance.GetProductUserId()
                };
                var lobbyInterface = EOSManager.Instance.GetEOSLobbyInterface();
                ResultE result = lobbyInterface.GetRTCRoomName(ref options, out Utf8String roomName);

                if(result != ResultE.Success){
                    Debug.LogErrorFormat("GetRTCRoomName: Could not get Room Name. Error Code: {0}", result);
                    return string.Empty;
                }
                return roomName;
            }
        }
        /// <summary>
        /// Notifications when a participant's status changes (oin or leave the room), or when the participant is added or removed from an applicable block list.
        /// </summary>
        /// <param name="data"></param>
        void OnParticipantStatusChanged(ref ParticipantStatusChangedCallbackInfo data){
            if (CurrentLobby.RTCRoomName != data.RoomName){
                Debug.LogError("OnParticipantStatusChanged: this room is invalid");
                return;
            }
            MemberState member = CurrentLobby.Members[UserId.GetUserId(data.ParticipantId).ToString()];

            if (data.ParticipantStatus == RTCParticipantStatus.Joined){
                member.RTCState.IsInRTCRoom = true;
            }else{
                member.RTCState.IsInRTCRoom = false;
                member.RTCState.IsSpeakinging = false;
            }
        }
        /// <summary>
        /// Notifications when a room participant audio status is updated.
        /// </summary>
        /// <param name="data"></param>
        void OnParticipantUpdated(ref ParticipantUpdatedCallbackInfo data){
            if (CurrentLobby.RTCRoomName != data.RoomName){
                Debug.LogError("OnParticipantUpdate: this room is invalid");
                return;
            }
        #if SYNICSUGAR_LOG
            Debug.LogFormat("OnParticipantUpdate: Change Paticipant State. UserId: {0} IsSpeaking: {1}", data.ParticipantId, data.Speaking);
        #endif
            MemberState member = CurrentLobby.Members[UserId.GetUserId(data.ParticipantId).ToString()];
            member.RTCState.IsSpeakinging = data.Speaking;
            member.RTCState.IsAudioOutputDisabled = data.AudioStatus != RTCAudioStatus.Enabled;
            if(data.Speaking){
                ParticipantUpdatedNotifier.OnStartTalking(UserId.GetUserId(data.ParticipantId));
            }else{
                ParticipantUpdatedNotifier.OnStopTalking(UserId.GetUserId(data.ParticipantId));
            }
        }
    #endregion
    #region Audio Send and Receive
        /// <summary>
        /// Starts local user sending voice chat. <br />
        /// This is enabled after a MatchMaking method returns true(= finish matchmaking). The receiving starts on StartPacketReceiver().
        /// </summary>
        public void StartVoiceSending(){
            if(!CurrentLobby.bEnableRTCRoom){
                Debug.LogError("StartVoiceSending: This lobby doesn't have RTC room.");
                return;
            }
            if(UseOpenVC){
                ToggleLocalUserSending(true);
            }else{
                StartAcceptingToPushToTalk();
            }
        }
        /// <summary>
        /// Stop local user sending voice chat. (= Mute) <br />
        /// </summary>
        public void StopVoiceSending(){
            if(!CurrentLobby.bEnableRTCRoom){
                Debug.LogError("StartVoiceSending: This lobby doesn't have RTC room.");
                return;
            }
            if(UseOpenVC){
                ToggleLocalUserSending(false);
            }else{
                StopAcceptingToPushToTalk();
            }
        }
        /// <summary>
        /// Switch Input setting of Local user sending on this Session.
        /// </summary>
        /// <param name="isEnable">If true, send VC. If false, stop VC.</param>
        void ToggleLocalUserSending(bool isEnable){
            if(!CurrentLobby.isValid() || System.String.IsNullOrEmpty(CurrentLobby.RTCRoomName)){
                Debug.LogError("MuteSendingOfLocalUserOnSession: the room is invalid.");
                return;
            }
            var sendingOptions = new UpdateSendingOptions(){
                LocalUserId = EOSManager.Instance.GetProductUserId(),
                RoomName = CurrentLobby.RTCRoomName,
                AudioStatus = isEnable ? RTCAudioStatus.Enabled : RTCAudioStatus.Disabled
            };
            audioInterface.UpdateSending(ref sendingOptions, null, OnUpdateSending);
        }
        void OnUpdateSending(ref UpdateSendingCallbackInfo info){
            if(info.ResultCode != ResultE.Success){
                Debug.LogErrorFormat("OnUpdateSending: could not toggle mute setting of local user sending. : {0}", info.ResultCode);
                return;
            }
    #if SYNICSUGAR_LOG
            Debug.Log("OnUpdateSending: the toggle is successful.");
    #endif
        }
        /// <summary>
        /// Switch Output setting(Enable or Mute) of receiving from target user on this Session.
        /// </summary>
        /// <param name="targetId">If null, effect to all remote users</param>
        /// <param name="isEnable">If true, receive vc from target. If false, mute target.</param>
        public void ToggleReceiveingFromTarget(UserId targetId, bool isEnable){
            if(!CurrentLobby.isValid() || System.String.IsNullOrEmpty(CurrentLobby.RTCRoomName)){
                Debug.LogError("ToggleReceiveingFromTargetUser: the room is invalid.");
                return;
            }
            Debug.Log(targetId == null);
            var receiveOptions = new UpdateReceivingOptions(){
                LocalUserId = EOSManager.Instance.GetProductUserId(),
                RoomName = CurrentLobby.RTCRoomName,
                ParticipantId = targetId == null ? null : targetId.AsEpic,
                AudioEnabled = isEnable
            };
            audioInterface.UpdateReceiving(ref receiveOptions, null, OnUpdateReceiving);
        }
        void OnUpdateReceiving(ref UpdateReceivingCallbackInfo info){
            if(info.ResultCode != ResultE.Success){
                Debug.LogErrorFormat("OnUpdateReceiving: could not toggle setting. : {0}", info.ResultCode);
                return;
            }
    #if SYNICSUGAR_LOG
            Debug.Log("OnUpdateReceiving: the toggle is successful.");
    #endif
        }
        /// <summary>
        /// Change the receiving volume on this Session.
        /// </summary>
        /// <param name="targetId">If null, effect to all remote users</param>
        /// <param name="volume">Range 0.0 - 100. 50 means that the audio volume is not modified and stays in its source value.</param>
        public void UpdateReceiveingVolumeFromTarget(UserId targetId, float volume){
            if(!CurrentLobby.isValid() || System.String.IsNullOrEmpty(CurrentLobby.RTCRoomName)){
                Debug.LogError("ToggleReceiveingFromTargetUser: the room is invalid.");
                return;
            }
            if(volume < 0){
                volume = 0f;
            }
            if(volume > 100){
                volume = 100f;
            }
            var receiveOptions = new UpdateParticipantVolumeOptions(){
                LocalUserId = EOSManager.Instance.GetProductUserId(),
                RoomName = CurrentLobby.RTCRoomName,
                ParticipantId = targetId == null ? null : targetId.AsEpic,
                Volume = volume
            };
            audioInterface.UpdateParticipantVolume(ref receiveOptions, null, OnUpdateParticipantVolume);
        }
        void OnUpdateParticipantVolume(ref UpdateParticipantVolumeCallbackInfo info){
            if(info.ResultCode != ResultE.Success){
                Debug.LogErrorFormat("OnUpdateParticipantVolume: could not toggle setting. : {0}", info.ResultCode);
                return;
            }
    #if SYNICSUGAR_LOG
            Debug.LogFormat("OnUpdateParticipantVolume: volume change is successful. target: {0} / Volume:{1}", info.ParticipantId, info.Volume);
    #endif
        }
        /// <summary>
        /// Start to accept PushToTalk key. <br />
        /// This switches the sending state with ToggleLocalUserSending() on user's input automatically.
        /// </summary>
        void StartAcceptingToPushToTalk(){
            if(pttToken != null && pttToken.Token.CanBeCanceled){
                pttToken.Cancel();
            }
            pttToken = new();
            PushToTalkLoop(pttToken.Token).Forget();
        }
        /// <summary>
        /// Stop to accept PushToTalk key.
        /// </summary>
        void StopAcceptingToPushToTalk(){
            if(pttToken == null){
                return;
            }
            pttToken.Cancel();
        }
        /// <summary>
        /// Switch sending state by UniTask. 
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        async UniTask PushToTalkLoop(CancellationToken token){
            while(!token.IsCancellationRequested && !UseOpenVC){
#if ENABLE_LEGACY_INPUT_MANAGER
                await UniTask.WaitUntil(() => Input.GetKeyDown(KeyToPushToTalk), cancellationToken: token);
                if(UseOpenVC){ break; }
                ToggleLocalUserSending(true);
                await UniTask.WaitUntil(() => Input.GetKeyUp(KeyToPushToTalk), cancellationToken: token);
                if(UseOpenVC){ break; }
                ToggleLocalUserSending(false);
#else
                await UniTask.WaitUntil(() => Keyboard.current[KeyToPushToTalk].wasPressedThisFrame, cancellationToken: token);
                if(UseOpenVC){ break; }
                ToggleLocalUserSending(true);
                await UniTask.WaitUntil(() => Keyboard.current[KeyToPushToTalk].wasReleasedThisFrame, cancellationToken: token);
                if(UseOpenVC){ break; }
                ToggleLocalUserSending(false);
#endif
            }
        }
    #endregion
        public UserId LastStateUpdatedUserId { get { return ParticipantUpdatedNotifier.TargetId;} } 
        public bool LastStateUpdatedUserStartTalking { get { return ParticipantUpdatedNotifier.IsTalkling; } }
    }
}

