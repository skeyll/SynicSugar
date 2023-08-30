using PlayEveryWare.EpicOnlineServices;
using Epic.OnlineServices.RTC;
using Epic.OnlineServices.RTCAudio;
using UnityEngine;
using SynicSugar.P2P;
using ResultE = Epic.OnlineServices.Result;

namespace SynicSugar.RTC{
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
        RTCRoom CurrentRoom = new();
        RTCInterface rtcInterface;
        RTCAudioInterface audioInterface;
        
        internal void UpdateLobbyInfo(){

        }
        /// <summary>
        /// Switch mute setting of Local user sending on just this SESSION.
        /// </summary>
        /// <param name="isMute"></param>
        public void ToggleLocalUserSendingMute(bool isMute){
            if(!CurrentRoom.isValid()){
                Debug.LogError("MuteSendingOfLocalUserOnSession: room is invalid.");
                return;
            }
            var sendingOptions = new UpdateSendingOptions(){
                LocalUserId = EOSManager.Instance.GetProductUserId(),
                RoomName = CurrentRoom.RoomName,
                AudioStatus = isMute ? RTCAudioStatus.Disabled : RTCAudioStatus.Disabled
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
        /// Switch mute setting of receiving from target user on just this SESSION.
        /// </summary>
        /// <param name="targetId">if null, effect for all remote users</param>
        /// <param name="isMute"></param>
        public void ToggleReceivedFromTargetUserMute(UserId targetId, bool isMute){
            if(!CurrentRoom.isValid()){
                Debug.LogError("ToggleReceivedFromTargetUserMute: room is invalid.");
                return;
            }
            var receiveOptions = new UpdateReceivingOptions(){
                LocalUserId = EOSManager.Instance.GetProductUserId(),
                RoomName = CurrentRoom.RoomName,
                ParticipantId = targetId != null ? targetId.AsEpic : null,
                AudioEnabled = !isMute
            };
            audioInterface.UpdateReceiving(ref receiveOptions, null, OnUpdateReceiving);
        }
        void OnUpdateReceiving(ref UpdateReceivingCallbackInfo info){
            if(info.ResultCode != ResultE.Success){
                Debug.LogErrorFormat("OnUpdateReceiving: could not toggle mute setting. : {0}", info.ResultCode);
                return;
            }
    #if SYNICSUGAR_LOG
            Debug.Log("OnUpdateReceiving: the toggle is successful.");
    #endif
        }
    }
}

