using PlayEveryWare.EpicOnlineServices;
using Epic.OnlineServices;
using Epic.OnlineServices.Lobby;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using SynicSugar.RTC;
using ResultE = Epic.OnlineServices.Result;
using SynicSugar.P2P;
using System;

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
        public List<AttributeData> Attributes = new List<AttributeData>();
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
 
        internal Dictionary<string, MemberState> Members = new();

        // Utility data
        internal bool _BeingCreated = false;

        #region RTC
        internal string RTCRoomName = System.String.Empty;
        internal bool hasConnectedRTCRoom = false;
        //for joing or leaving
        internal NotifyEventHandle RTCParticipantStatusChanged; 
        //for speaking or non-speaking
        internal NotifyEventHandle RTCParticipantUpdated;
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
            RTCManager.Instance.RemoveRTCEvents();
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

            CopyLobbyDetailsHandleOptions options = new CopyLobbyDetailsHandleOptions(){
                LobbyId = LobbyId,
                LocalUserId = EOSManager.Instance.GetProductUserId()
            };

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
        /// <summary>
        /// Initializing the given LobbyDetails handle and caches all relevant attributes
        /// </summary>
        /// <param name="outLobbyDetailsHandle">Specified LobbyDetails handle</param>
        internal void InitFromLobbyDetails(LobbyDetails outLobbyDetailsHandle){
            // Get owner
            var lobbyDetailsGetLobbyOwnerOptions = new LobbyDetailsGetLobbyOwnerOptions();
            ProductUserId newLobbyOwner = outLobbyDetailsHandle.GetLobbyOwner(ref lobbyDetailsGetLobbyOwnerOptions);
            if (newLobbyOwner != LobbyOwner){
                LobbyOwner = newLobbyOwner;
            }

            // Copy lobby info
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

            // Get attributes
            Attributes.Clear();
            var lobbyDetailsGetAttributeCountOptions = new LobbyDetailsGetAttributeCountOptions();
            uint attrCount = outLobbyDetailsHandle.GetAttributeCount(ref lobbyDetailsGetAttributeCountOptions);
            for (uint i = 0; i < attrCount; i++){
                LobbyDetailsCopyAttributeByIndexOptions attrOptions = new LobbyDetailsCopyAttributeByIndexOptions();
                attrOptions.AttrIndex = i;
                ResultE copyAttrResult = outLobbyDetailsHandle.CopyAttributeByIndex(ref attrOptions, out Epic.OnlineServices.Lobby.Attribute? outAttribute);
                if (copyAttrResult == ResultE.Success && outAttribute != null && outAttribute?.Data != null){
                    AttributeData attr = EOSLobbyExtensions.GenerateLobbyAttribute(outAttribute);
                    Attributes.Add(attr);
                }
            }

            // Get members
            // List<LobbyMember> OldMembers = new List<LobbyMember>(Members);
            Dictionary<string, MemberState> tmp = new (Members);
            Members.Clear();

            var lobbyDetailsGetMemberCountOptions = new LobbyDetailsGetMemberCountOptions();
            uint memberCount = outLobbyDetailsHandle.GetMemberCount(ref lobbyDetailsGetMemberCountOptions);

            for (int memberIndex = 0; memberIndex < memberCount; memberIndex++){
                var lobbyDetailsGetMemberByIndexOptions = new LobbyDetailsGetMemberByIndexOptions() { MemberIndex = (uint)memberIndex };
                ProductUserId memberId = outLobbyDetailsHandle.GetMemberByIndex(ref lobbyDetailsGetMemberByIndexOptions);
                Members.Add(UserId.GetUserId(memberId).ToString(), new MemberState(){});

                // Add member attributes
                var lobbyDetailsGetMemberAttributeCountOptions = new LobbyDetailsGetMemberAttributeCountOptions() { TargetUserId = memberId };
                int memberAttributeCount = (int)outLobbyDetailsHandle.GetMemberAttributeCount(ref lobbyDetailsGetMemberAttributeCountOptions);

                for (int attributeIndex = 0; attributeIndex < memberAttributeCount; attributeIndex++){
                    var lobbyDetailsCopyMemberAttributeByIndexOptions = new LobbyDetailsCopyMemberAttributeByIndexOptions() { AttrIndex = (uint)attributeIndex, TargetUserId = memberId };
                    ResultE memberAttributeResult = outLobbyDetailsHandle.CopyMemberAttributeByIndex(ref lobbyDetailsCopyMemberAttributeByIndexOptions, out Epic.OnlineServices.Lobby.Attribute? outAttribute);

                    if (memberAttributeResult != ResultE.Success){
                        Debug.LogFormat("Lobbies (InitFromLobbyDetails): can't copy member attribute. Error code: {0}", memberAttributeResult);
                        continue;
                    }

                    AttributeData newAttribute = EOSLobbyExtensions.GenerateLobbyAttribute(outAttribute);
 
                    Members[memberId.ToString()].Attributes.Add(newAttribute);
                }
            }
        }
    }
    /// <summary>
    /// Class represents all Lobby Member properties
    /// </summary>
    internal class MemberState {
        public List<AttributeData> Attributes { get; internal set; } = new List<AttributeData>();    
        public RTCState RTCState { get; internal set; } = new RTCState();

        internal AttributeData GetAttributeData(string Key){
            foreach (var attribute in Attributes){
                if(attribute.Key == Key){
                    return attribute;
                }
            }
            //No data
            return null;
        }
    }

    public class RTCState {
        public bool IsInRTCRoom { get; internal set; } = false;
        public bool IsSpeakinging { get; internal set; } = false;
        public bool IsAudioOutputEnabled { get; internal set; } = false;
        public bool IsHardMuted { get; internal set; } = false;
        public bool IsLocalMute { get; internal set; } = false;
        public float LocalOutputedVolume { get; internal set; } = 50.0f;
    }
    /// <summary>
    /// Lobby and Member Attribute data
    /// </summary>
    public class AttributeData {
        internal LobbyAttributeVisibility Visibility = LobbyAttributeVisibility.Public;
        
        public string Key;
        //Only one of the following properties will have valid data (depending on 'ValueType')
        public bool? BOOLEAN { get; private set; }
        public int? INT64 { get; private set; } = 0;
        public double? DOUBLE { get; private set; } = 0.0;
        public string STRING { get; private set; }
        public AttributeType ValueType { get; private set; } = AttributeType.String;
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
        public string GetValueAsString(){
            switch(ValueType){
                case AttributeType.Boolean:
                return BOOLEAN.ToString();
                case AttributeType.Int64:
                return INT64.ToString();
                case AttributeType.Double:
                return DOUBLE.ToString();
                case AttributeType.String:
                return STRING;
            }
            return System.String.Empty;
        }
        public override int GetHashCode(){
            return base.GetHashCode();
        }
    }
    [Obsolete("This is old. AttributeData is new one.")]
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
        public MatchGUIState(){}
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