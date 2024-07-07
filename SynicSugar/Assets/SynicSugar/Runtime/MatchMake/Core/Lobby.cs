using PlayEveryWare.EpicOnlineServices;
using Epic.OnlineServices;
using Epic.OnlineServices.Lobby;
using System.Collections.Generic;
using UnityEngine;
using SynicSugar.RTC;
using ResultE = Epic.OnlineServices.Result;
using SynicSugar.P2P;

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
        public string BucketId = string.Empty;
        internal bool bAllowInvites = false;
        internal bool bDisableHostMigration = true;
        internal bool bEnableRTCRoom = false;
        /// <summary>
        /// If true, the pleyer who was kicked need Invite to join the same Lobby.
        /// </summary>
        public bool RejoinAfterKickRequiresInvite = false;
        public List<AttributeData> Attributes = new List<AttributeData>();
        internal uint AvailableSlots = 0;
        internal void SetBucketID(string[] conditions){
            BucketId = string.Empty;
            if(conditions.Length == 0){
                BucketId = "NONE";
                return;
            }
            for(int i = 0; i < conditions.Length; i++){
                BucketId += i == 0 ? conditions[i] : (":" + conditions[i]);
            }
        }
 
        internal Dictionary<string, MemberState> Members = new();

        // Return True only when create Lobby. Otherwise, return False because this is re-created by constructor.
        internal bool _BeingCreated = false;

        #region RTC
        internal string RTCRoomName = string.Empty;
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
}