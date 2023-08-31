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
        /// Register events and notify to use RTC.<br />
        /// Must call this to use after Created ot Join Lobby.
        /// </summary>
        internal void SubscribeToRTCEvents(){
            //Need RTC?
            if(!CurrentLobby.bEnableRTCRoom){
                return;
            }
            CurrentLobby.RTCRoomName = GetRTCRoomName();
            if(System.String.IsNullOrEmpty(CurrentLobby.RTCRoomName)){
                Debug.LogError("SubscribeToRTCEvents: This user does't have not find RTC room.");
                return;
            }
            //Validation of RTC room
            LobbyInterface lobbyInterface = EOSManager.Instance.GetEOSLobbyInterface();

            IsRTCRoomConnectedOptions isConnectedOptions = new IsRTCRoomConnectedOptions(){
                LobbyId = CurrentLobby.LobbyId,
                LocalUserId = EOSManager.Instance.GetProductUserId()
            };
            ResultE result = lobbyInterface.IsRTCRoomConnected(ref isConnectedOptions, out bool isConnected);
            if (result != ResultE.Success){
                Debug.LogFormat("SubscribeToRTCEvents: This user is not participating in the RTC room.: {0}", result);
                return;
            }
            CurrentLobby.hasConnectedRTCRoom = isConnected;
            

            //Notify to get user talking status
            RTCAudioInterface rtcAudioInterface = rtcInterface.GetAudioInterface();
            AddNotifyParticipantUpdatedOptions addNotifyParticipantUpdatedOptions = new AddNotifyParticipantUpdatedOptions(){
                LocalUserId = EOSManager.Instance.GetProductUserId(),
                RoomName = CurrentLobby.RTCRoomName
            };

            CurrentLobby.RTCParticipantUpdated = new NotifyEventHandle(rtcAudioInterface.AddNotifyParticipantUpdated(ref addNotifyParticipantUpdatedOptions, null, OnRTCRoomParticipantUpdate), (ulong handle) =>{
                EOSManager.Instance.GetEOSRTCInterface().GetAudioInterface().RemoveNotifyParticipantUpdated(handle);
            });
            //Start Voice Chat
            if(UseOpenVC){
                ToggleLocalUserSending(true);
            }else{
                StartAcceptingToPushToTalk();
            }
            ToggleReceiveingFromTargetUser(null, true);

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
        /// Call this close or leave lobby.
        /// </summary>
        internal void UnsubscribeFromRTCEvents(){
            if(!CurrentLobby.bEnableRTCRoom){
                return;
            }
            CurrentLobby.RTCParticipantStatusChanged.Dispose();
            CurrentLobby.RTCParticipantUpdated.Dispose();

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
            //Check audio devices
            var audioOptions = new GetAudioInputDevicesCountOptions();
            audioInterface.GetAudioInputDevicesCount(ref audioOptions);

            var audioOutputOptions = new GetAudioOutputDevicesCountOptions();
            audioInterface.GetAudioOutputDevicesCount(ref audioOutputOptions);

            // Notify to get a user's joining and leaving
            RTCInterface rtcInterface = EOSManager.Instance.GetEOSRTCInterface();

            AddNotifyParticipantStatusChangedOptions addNotifyParticipantsStatusChangedOptions = new AddNotifyParticipantStatusChangedOptions(){
                LocalUserId = EOSManager.Instance.GetProductUserId(),
                RoomName = CurrentLobby.RTCRoomName
            };
            CurrentLobby.RTCParticipantStatusChanged = new NotifyEventHandle(rtcInterface.AddNotifyParticipantStatusChanged(ref addNotifyParticipantsStatusChangedOptions, null, OnRTCRoomParticipantStatusChanged), (ulong handle) =>{
                EOSManager.Instance.GetEOSRTCInterface().RemoveNotifyParticipantStatusChanged(handle);
            });
            if(!CurrentLobby.RTCParticipantStatusChanged.IsValid()){
                Debug.LogError("AddNotifyParticipantStatusChanged: RTCRoomParticipantUpdate isn't regisered.");
            }
        }
        /// <summary>
        /// Notifications when a participant's status changes (oin or leave the room), or when the participant is added or removed from an applicable block list.
        /// </summary>
        /// <param name="data"></param>
        void OnRTCRoomParticipantStatusChanged(ref ParticipantStatusChangedCallbackInfo data){
            if (string.IsNullOrEmpty(CurrentLobby.RTCRoomName) || CurrentLobby.RTCRoomName == data.RoomName){
                Debug.LogError("OnRTCRoomParticipantStatusChanged: this room is invalid");
                return;
            }

            foreach (var member in CurrentLobby.Members){
                if(member.ProductId != data.ParticipantId){
                    continue;
                }
                if (data.ParticipantStatus == RTCParticipantStatus.Joined){
                    member.RTCState.IsInRTCRoom = true;
                }else{
                    member.RTCState.IsInRTCRoom = false;
                    member.RTCState.IsTalking = false;
                }
                break;
            }
        }
        /// <summary>
        /// Notifications when a room participant audio status is updated.
        /// </summary>
        /// <param name="data"></param>
        void OnRTCRoomParticipantUpdate(ref ParticipantUpdatedCallbackInfo data){
            if (string.IsNullOrEmpty(CurrentLobby.RTCRoomName) || CurrentLobby.RTCRoomName == data.RoomName){
                Debug.LogError("OnRTCRoomParticipantUpdate: this room is invalid");
                return;
            }

            foreach(var member in CurrentLobby.Members){
                if(member.ProductId != data.ParticipantId){
                    continue;
                }
                member.RTCState.IsTalking = data.Speaking;
                member.RTCState.IsAudioOutputDisabled = data.AudioStatus != RTCAudioStatus.Enabled;
                break;
            }
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
        /// Switch Output setting of receiving from target user on this SESSION.
        /// </summary>
        /// <param name="targetId">if null, effect for all remote users</param>
        /// <param name="isEnable">If true, receive vc from target. If false, mute target.</param>
        public void ToggleReceiveingFromTargetUser(UserId targetId, bool isEnable){
            if(!CurrentLobby.isValid() || System.String.IsNullOrEmpty(CurrentLobby.RTCRoomName)){
                Debug.LogError("ToggleReceiveingFromTargetUser: the room is invalid.");
                return;
            }
            var receiveOptions = new UpdateReceivingOptions(){
                LocalUserId = EOSManager.Instance.GetProductUserId(),
                RoomName = CurrentLobby.RTCRoomName,
                ParticipantId = targetId != null ? targetId.AsEpic : null,
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
            while(!token.IsCancellationRequested && !RTCManager.Instance.UseOpenVC){
#if ENABLE_LEGACY_INPUT_MANAGER
                await UniTask.WaitUntil(() => Input.GetKeyDown(KeyToPushToTalk), cancellationToken: token);
                if(RTCManager.Instance.UseOpenVC){ break; }
                ToggleLocalUserSending(true);
                await UniTask.WaitUntil(() => Input.GetKeyUp(KeyToPushToTalk), cancellationToken: token);
                if(RTCManager.Instance.UseOpenVC){ break; }
                ToggleLocalUserSending(false);
#else
                await UniTask.WaitUntil(() => Keyboard.current[KeyToPushToTalk].wasPressedThisFrame, cancellationToken: token);
                if(RTCManager.Instance.UseOpenVC){ break; }
                ToggleLocalUserSending(true);
                await UniTask.WaitUntil(() => Keyboard.current[KeyToPushToTalk].wasReleasedThisFrame, cancellationToken: token);
                if(RTCManager.Instance.UseOpenVC){ break; }
                ToggleLocalUserSending(false);
#endif
            }
        }
    }
}

