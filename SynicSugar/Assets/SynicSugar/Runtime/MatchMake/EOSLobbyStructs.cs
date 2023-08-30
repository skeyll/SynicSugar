using PlayEveryWare.EpicOnlineServices;
using Epic.OnlineServices;
using Epic.OnlineServices.Lobby;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using ResultE = Epic.OnlineServices.Result;
using SynicSugar.RTC;

namespace SynicSugar.MatchMake {
    public class Lobby {
        internal string LobbyId;
        internal ProductUserId LobbyOwner = new ProductUserId();
        //Basis
        uint _maxLobbyMembers;
        public uint MaxLobbyMembers { //2-64
            get { return _maxLobbyMembers;}
            set {
                if(value < 2){
                    _maxLobbyMembers = 2;
                }else if(value > 64){
                    _maxLobbyMembers = 64;
                }else{
                    _maxLobbyMembers = value;
                }
            }
        }
        internal LobbyPermissionLevel PermissionLevel = LobbyPermissionLevel.Publicadvertised;
        public string BucketId = System.String.Empty;
        internal bool bAllowInvites = false;
        internal bool bDisableHostMigration = true;
        internal bool bEnableRTCRoom = false;
        public List<LobbyAttribute> Attributes = new List<LobbyAttribute>();
        internal uint AvailableSlots = 0;
        internal void SetBucketID(string[] conditions){
            BucketId = System.String.Empty;
            if(conditions.Length == 0){
                BucketId = "NONE";
                return;
            }
            for(int i = 0; i < conditions.Length; i++){
                BucketId += i == 0 ? conditions[i] : (":" + conditions[i]);
            }
        }

        internal List<LobbyMember> Members = new List<LobbyMember>();

        // Utility data
        internal bool _BeingCreated = false;

        #region RTC
        internal string RTCRoomName = System.String.Empty;
        internal bool hasConnectedRTCRoom = false;
        //new players or players leaving
        internal NotifyEventHandle RTCParticipantStatusChanged; 
        //talking status or mute changes
        internal NotifyEventHandle RTCRoomParticipantAudioUpdate;
        #endregion

        /// <summary>
        /// Checks if Lobby Id is valid
        /// </summary>
        /// <returns>True if valid</returns>
        internal bool isValid(){
            return !string.IsNullOrEmpty(LobbyId);
        }

        /// <summary>
        /// Checks the local player is the lobby host or not.
        /// </summary>
        /// <returns>If true, the local user is lobby's Host</returns>
        internal bool isHost(){
            return EOSManager.Instance.GetProductUserId() == LobbyOwner;
        }

        /// <summary>
        /// Clears local cache of Lobby Id, owner, attributes and members
        /// </summary>
        internal void Clear(){
            LobbyId = string.Empty;
            LobbyOwner = new ProductUserId();
            Attributes.Clear();
            Members.Clear();
            RTCManager.Instance.UnsubscribeFromRTCEvents();
        }

        /// <summary>
        /// Initializing the given Lobby Id and caches all relevant attributes
        /// </summary>
        /// <param name="lobbyId">Specified Lobby Id</param>
        internal void InitFromLobbyHandle(string lobbyId){
            if (string.IsNullOrEmpty(lobbyId)){
                return;
            }

            LobbyId = lobbyId;

            CopyLobbyDetailsHandleOptions options = new CopyLobbyDetailsHandleOptions();
            options.LobbyId = LobbyId;
            options.LocalUserId = EOSManager.Instance.GetProductUserId();

            ResultE result = EOSManager.Instance.GetEOSLobbyInterface().CopyLobbyDetailsHandle(ref options, out LobbyDetails outLobbyDetailsHandle);
            if (result != ResultE.Success){
                Debug.LogErrorFormat("Init Lobby: can't get lobby info handle. Error code: {0}", result);
                return;
            }
            if (outLobbyDetailsHandle == null){
                Debug.LogError("Init Lobby: can't get lobby info handle. outLobbyDetailsHandle is null");
                return;
            }

            InitFromLobbyDetails(outLobbyDetailsHandle);
        }

        //-------------------------------------------------------------------------
        /// <summary>
        /// Initializing the given <c>LobbyDetails</c> handle and caches all relevant attributes
        /// </summary>
        /// <param name="lobbyId">Specified <c>LobbyDetails</c> handle</param>
        internal void InitFromLobbyDetails(LobbyDetails outLobbyDetailsHandle){
            // get owner
            var lobbyDetailsGetLobbyOwnerOptions = new LobbyDetailsGetLobbyOwnerOptions();
            ProductUserId newLobbyOwner = outLobbyDetailsHandle.GetLobbyOwner(ref lobbyDetailsGetLobbyOwnerOptions);
            if (newLobbyOwner != LobbyOwner){
                LobbyOwner = newLobbyOwner;
            }

            // copy lobby info
            var lobbyDetailsCopyInfoOptions = new LobbyDetailsCopyInfoOptions();
            ResultE infoResult = outLobbyDetailsHandle.CopyInfo(ref lobbyDetailsCopyInfoOptions, out LobbyDetailsInfo? outLobbyDetailsInfo);
            if (infoResult != ResultE.Success){
                Debug.LogErrorFormat("Init Lobby: can't copy lobby info. Error code: {0}", infoResult);
                return;
            }
            if (outLobbyDetailsInfo == null){
                Debug.LogError("Init Lobby: could not copy info: outLobbyDetailsInfo is null.");
                return;
            }

            LobbyId = outLobbyDetailsInfo?.LobbyId;
            MaxLobbyMembers = (uint)(outLobbyDetailsInfo?.MaxMembers);
            PermissionLevel = (LobbyPermissionLevel)(outLobbyDetailsInfo?.PermissionLevel);
            bAllowInvites = (bool)(outLobbyDetailsInfo?.AllowInvites);
            bEnableRTCRoom = (bool)(outLobbyDetailsInfo?.RTCRoomEnabled);
            AvailableSlots = (uint)(outLobbyDetailsInfo?.AvailableSlots);
            BucketId = outLobbyDetailsInfo?.BucketId;

            // get attributes
            Attributes.Clear();
            var lobbyDetailsGetAttributeCountOptions = new LobbyDetailsGetAttributeCountOptions();
            uint attrCount = outLobbyDetailsHandle.GetAttributeCount(ref lobbyDetailsGetAttributeCountOptions);
            for (uint i = 0; i < attrCount; i++){
                LobbyDetailsCopyAttributeByIndexOptions attrOptions = new LobbyDetailsCopyAttributeByIndexOptions();
                attrOptions.AttrIndex = i;
                ResultE copyAttrResult = outLobbyDetailsHandle.CopyAttributeByIndex(ref attrOptions, out Epic.OnlineServices.Lobby.Attribute? outAttribute);
                if (copyAttrResult == ResultE.Success && outAttribute != null && outAttribute?.Data != null){
                    LobbyAttribute attr = EOSLobbyExtensions.GenerateLobbyAttribute(outAttribute);
                    Attributes.Add(attr);
                }
            }

            // get members
            List<LobbyMember> OldMembers = new List<LobbyMember>(Members);
            Members.Clear();

            var lobbyDetailsGetMemberCountOptions = new LobbyDetailsGetMemberCountOptions();
            uint memberCount = outLobbyDetailsHandle.GetMemberCount(ref lobbyDetailsGetMemberCountOptions);

            for (int memberIndex = 0; memberIndex < memberCount; memberIndex++){
                var lobbyDetailsGetMemberByIndexOptions = new LobbyDetailsGetMemberByIndexOptions() { MemberIndex = (uint)memberIndex };
                ProductUserId memberId = outLobbyDetailsHandle.GetMemberByIndex(ref lobbyDetailsGetMemberByIndexOptions);
                Members.Insert((int)memberIndex, new LobbyMember() { ProductId = memberId });

                // member attributes
                var lobbyDetailsGetMemberAttributeCountOptions = new LobbyDetailsGetMemberAttributeCountOptions() { TargetUserId = memberId };
                int memberAttributeCount = (int)outLobbyDetailsHandle.GetMemberAttributeCount(ref lobbyDetailsGetMemberAttributeCountOptions);

                for (int attributeIndex = 0; attributeIndex < memberAttributeCount; attributeIndex++){
                    var lobbyDetailsCopyMemberAttributeByIndexOptions = new LobbyDetailsCopyMemberAttributeByIndexOptions() { AttrIndex = (uint)attributeIndex, TargetUserId = memberId };
                    ResultE memberAttributeResult = outLobbyDetailsHandle.CopyMemberAttributeByIndex(ref lobbyDetailsCopyMemberAttributeByIndexOptions, out Epic.OnlineServices.Lobby.Attribute? outAttribute);

                    if (memberAttributeResult != ResultE.Success){
                        Debug.LogFormat("Lobbies (InitFromLobbyDetails): can't copy member attribute. Error code: {0}", memberAttributeResult);
                        continue;
                    }

                    LobbyAttribute newAttribute = EOSLobbyExtensions.GenerateLobbyAttribute(outAttribute);
 
                    Members[memberIndex].MemberAttributes.Add(newAttribute.Key, newAttribute);
                }
            }
        }
    }
    /// <summary>
    /// Class represents all Lobby Member properties
    /// </summary>
    internal class LobbyMember {
        public ProductUserId ProductId;
        public Dictionary<string, LobbyAttribute> MemberAttributes = new Dictionary<string, LobbyAttribute>();
        
        public LobbyRTCState RTCState = new LobbyRTCState();
    }

    public class LobbyRTCState{
        // Is this person currently connected to the RTC room?
        public bool IsInRTCRoom = false;
        // Is this person currently talking (audible sounds from their audio output)
        public bool IsTalking = false;
        // We have locally muted this person (others can still hear them)
        public bool IsLocalMuted = false;
        // Has this person muted their own audio output (nobody can hear them)
        public bool IsAudioOutputDisabled = false;
        // Are we currently muting this person?
        public bool MuteActionInProgress = false;
        // Has this person enabled press to talk
        public bool PressToTalkEnabled = false;
    }
    /// <summary>
    /// Class represents all Lobby Attribute properties
    /// </summary>
    public class LobbyAttribute {
        internal LobbyAttributeVisibility Visibility = LobbyAttributeVisibility.Public;
        
        public string Key;
        //Only one of the following properties will have valid data (depending on 'ValueType')
        internal bool? BOOLEAN { get; private set; }
        internal int? INT64 { get; private set; } = 0;
        internal double? DOUBLE { get; private set; } = 0.0;
        internal string STRING { get; private set; }
        internal AttributeType ValueType { get; private set; } = AttributeType.String;
        public ComparisonOp ComparisonOperator = ComparisonOp.Equal;
        /// <summary>
        /// Can use bool, int, double and string.
        /// Retrun new whole attribute instanse by GenereteSerssionAttribute<T>(Key, Value, advertiseType).
        /// </summary>
        /// <param name="value"></param>
        public void SetValue(bool value){
            BOOLEAN = value;
            ValueType = AttributeType.Boolean;
        }
        public void SetValue(int value){
            INT64 = value;
            ValueType = AttributeType.Int64;
        }
        public void SetValue(double value){
            DOUBLE = value;
            ValueType = AttributeType.Double;
        }
        public void SetValue(string value){
            STRING = value;
            ValueType = AttributeType.String;
        }

        public override int GetHashCode(){
            return base.GetHashCode();
        }
    }
    
    public enum MatchState {
        Search, Wait, Connect, Success, Fail, Cancel
    }
    [System.Serializable]
    public class MatchGUIState {
        public MatchGUIState(){

        }
        public MatchGUIState(Text uiText){
            state = uiText;
        }
        public Text state;
        //ex.
        // 1. Press [start match make] button.
        // 2. Make [start match make] disable not to press multiple times. -> stopAdditionalInput
        // 3. Change [start match make] text to [stop match make]. -> acceptCancel
        // 4. (On Success) Completely inactive [start match make]. -> stopAdditionalInput
        public UnityEvent stopAdditionalInput = new UnityEvent();
        public UnityEvent acceptCancel = new UnityEvent();
        //Diplay these on UI text.
        public string searchLobby, waitothers, tryconnect, success, fail, trycancel;
        internal string GetDiscription(MatchState state){
            switch(state){
                case MatchState.Search:
                return searchLobby;
                case MatchState.Wait:
                return waitothers;
                case MatchState.Connect:
                return tryconnect;
                case MatchState.Success:
                return success;
                case MatchState.Cancel:
                return trycancel;
            }
            
            return System.String.Empty;
        }
    }
}