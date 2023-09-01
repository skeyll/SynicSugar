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
                Instance = null;
            }
        }
        // RTCRoom CurrentRoom = new();
        Lobby CurrentLobby { get { return MatchMakeManager.Instance.eosLobby.CurrentLobby; }}
        RTCInterface rtcInterface;
        RTCAudioInterface audioInterface;
        CancellationTokenSource pttToken;
        ulong ParticipantStatusId, ParticipantUpdatedId;
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
    #region RTC Notify
        /// <summary>
        /// Register event to get VC status, then start VC.<br />
        /// Must call this to use after Created ot Join Lobby.
        /// </summary>
        internal void StartVoiceChat(){
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
                ParticipantUpdatedId = audioInterface.AddNotifyParticipantUpdated(ref addNotifyParticipantUpdatedOptions, null, OnRTCRoomParticipantUpdate);
                if(ParticipantUpdatedId == 0){
                    Debug.LogError("StartVoiceChat: AddNotifyParticipantUpdated can not be regisered.");
                    return;
                }
            }
            //Start Voice Chat
            if(UseOpenVC){
                ToggleLocalUserSending(true);
            }else{
                StartAcceptingToPushToTalk();
            }
            ToggleReceiveingFromTarget(null, true);
            // ToggleLocalUserReceiveing(true);
        }
        /// <summary>
        /// Call this close or leave lobby.
        /// </summary>
        internal void UnsubscribeFromRTCEvents(){
            if(!CurrentLobby.bEnableRTCRoom){
                return;
            }
            // CurrentLobby.RTCParticipantStatusChanged.Dispose();
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
                ParticipantStatusId = rtcInterface.AddNotifyParticipantStatusChanged(ref StatusChangedOptions, null, OnRTCRoomParticipantStatusChanged);

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
        void OnRTCRoomParticipantStatusChanged(ref ParticipantStatusChangedCallbackInfo data){
            if (string.IsNullOrEmpty(CurrentLobby.RTCRoomName) || CurrentLobby.RTCRoomName != data.RoomName){
                Debug.LogError("OnRTCRoomParticipantStatusChanged: this room is invalid");
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
        void OnRTCRoomParticipantUpdate(ref ParticipantUpdatedCallbackInfo data){
            if (string.IsNullOrEmpty(CurrentLobby.RTCRoomName) || CurrentLobby.RTCRoomName != data.RoomName){
                Debug.LogError("OnRTCRoomParticipantUpdate: this room is invalid");
                return;
            }

            MemberState member = CurrentLobby.Members[UserId.GetUserId(data.ParticipantId).ToString()];
            member.RTCState.IsSpeakinging = data.Speaking;
            member.RTCState.IsAudioOutputDisabled = data.AudioStatus != RTCAudioStatus.Enabled;
        }
    #endregion
    #region Audio Send and Receive
        /// <summary>
        /// Switch Input setting of Local user sending on this SESSION.
        /// </summary>
        /// <param name="isEnable">If true, send VC. If false, stop VC.</param>
        public void ToggleLocalUserSending(bool isEnable){
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
        /// Switch Output settings of receiving from other all users on this SESSION.
        /// </summary>
        /// <param name="isEnable">If true, receive vc from target. If false, mute target.</param>
        public void ToggleLocalUserReceiveing(bool isEnable){
            if(!CurrentLobby.isValid() || System.String.IsNullOrEmpty(CurrentLobby.RTCRoomName)){
                Debug.LogError("ToggleReceiveingFromTargetUser: the room is invalid.");
                return;
            }
            var receiveOptions = new UpdateReceivingOptions(){
                LocalUserId = EOSManager.Instance.GetProductUserId(),
                RoomName = CurrentLobby.RTCRoomName,
                ParticipantId = null,
                AudioEnabled = isEnable
            };
            audioInterface.UpdateReceiving(ref receiveOptions, null, OnUpdateReceiving);
        }
        /// <summary>
        /// Switch Output setting of receiving from target user on this SESSION.
        /// </summary>
        /// <param name="targetId">if null, effect to all remote users</param>
        /// <param name="isEnable">If true, receive vc from target. If false, mute target.</param>
        public void ToggleReceiveingFromTarget(UserId targetId, bool isEnable){
            if(!CurrentLobby.isValid() || System.String.IsNullOrEmpty(CurrentLobby.RTCRoomName)){
                Debug.LogError("ToggleReceiveingFromTargetUser: the room is invalid.");
                return;
            }
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
    #endregion
        /// <summary>
        /// Start to accept PushToTalk key. <br />
        /// This switches the sending state with ToggleLocalUserSending() on user's input automatically.
        /// </summary>
        public void StartAcceptingToPushToTalk(){
            if(pttToken != null && pttToken.Token.CanBeCanceled){
                pttToken.Cancel();
            }
            pttToken = new();
            PushToTalkLoop(pttToken.Token).Forget();
        }
        /// <summary>
        /// Stop to accept PushToTalk key.
        /// </summary>
        public void StopAcceptingToPushToTalk(){
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
    }
}

