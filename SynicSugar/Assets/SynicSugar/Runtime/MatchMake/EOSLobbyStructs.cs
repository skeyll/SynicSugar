using PlayEveryWare.EpicOnlineServices;
using Epic.OnlineServices;
using Epic.OnlineServices.Lobby;
using System.Collections.Generic;
using UnityEngine;
namespace SynicSugar.MatchMake {
    public class Lobby {
        public string LobbyId;
        public ProductUserId LobbyOwner = new ProductUserId();
        //Basis
        public string LocalUserID;
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
        public LobbyPermissionLevel PermissionLevel = LobbyPermissionLevel.Publicadvertised;
        public string BucketId = System.String.Empty;
        public bool bPresenceEnabled = false;
        public bool bAllowInvites = false;
        public bool bDisableHostMigration = true;
        public List<LobbyAttribute> Attributes = new List<LobbyAttribute>();
        public uint AvailableSlots = 0;
        public void SetBucketID(string[] conditions){
            BucketId = System.String.Empty;
            for(int i = 0; i < conditions.Length; i++){
                if(conditions[i] != ""){
                    BucketId += i == 0 ? conditions[i] : (":" + conditions[i]);
                }
            }
        }

        public List<LobbyMember> Members = new List<LobbyMember>();

        // Utility data
        public bool _SearchResult = false;
        public bool _BeingCreated = false;

        /// <summary>
        /// Checks if Lobby Id is valid
        /// </summary>
        /// <returns>True if valid</returns>
        public bool isValid(){
            return !string.IsNullOrEmpty(LobbyId);
        }

        /// <summary>
        /// Checks if the specified <c>ProductUserId</c> is the current owner
        /// </summary>
        /// <param name="userProductId">Specified <c>ProductUserId</c></param>
        /// <returns>True if specified user is owner</returns>
        public bool isHost(ProductUserId userProductId){
            return userProductId == LobbyOwner;
        }

        /// <summary>
        /// Clears local cache of Lobby Id, owner, attributes and members
        /// </summary>
        public void Clear(){
            LobbyId = string.Empty;
            LobbyOwner = new ProductUserId();
            Attributes.Clear();
            Members.Clear();
        }

        /// <summary>
        /// Initializing the given Lobby Id and caches all relevant attributes
        /// </summary>
        /// <param name="lobbyId">Specified Lobby Id</param>
        public void InitFromLobbyHandle(string lobbyId){
            if (string.IsNullOrEmpty(lobbyId)){
                return;
            }

            LobbyId = lobbyId;

            CopyLobbyDetailsHandleOptions options = new CopyLobbyDetailsHandleOptions();
            options.LobbyId = LobbyId;
            options.LocalUserId = EOSManager.Instance.GetProductUserId();

            Result result = EOSManager.Instance.GetEOSLobbyInterface().CopyLobbyDetailsHandle(ref options, out LobbyDetails outLobbyDetailsHandle);
            if (result != Result.Success){
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
        public void InitFromLobbyDetails(LobbyDetails outLobbyDetailsHandle){
            // get owner
            var lobbyDetailsGetLobbyOwnerOptions = new LobbyDetailsGetLobbyOwnerOptions();
            ProductUserId newLobbyOwner = outLobbyDetailsHandle.GetLobbyOwner(ref lobbyDetailsGetLobbyOwnerOptions);
            if (newLobbyOwner != LobbyOwner){
                LobbyOwner = newLobbyOwner;
            }

            // copy lobby info
            var lobbyDetailsCopyInfoOptions = new LobbyDetailsCopyInfoOptions();
            Result infoResult = outLobbyDetailsHandle.CopyInfo(ref lobbyDetailsCopyInfoOptions, out LobbyDetailsInfo? outLobbyDetailsInfo);
            if (infoResult != Result.Success){
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
            AvailableSlots = (uint)(outLobbyDetailsInfo?.AvailableSlots);
            BucketId = outLobbyDetailsInfo?.BucketId;

            // get attributes
            Attributes.Clear();
            var lobbyDetailsGetAttributeCountOptions = new LobbyDetailsGetAttributeCountOptions();
            uint attrCount = outLobbyDetailsHandle.GetAttributeCount(ref lobbyDetailsGetAttributeCountOptions);
            for (uint i = 0; i < attrCount; i++){
                LobbyDetailsCopyAttributeByIndexOptions attrOptions = new LobbyDetailsCopyAttributeByIndexOptions();
                attrOptions.AttrIndex = i;
                Result copyAttrResult = outLobbyDetailsHandle.CopyAttributeByIndex(ref attrOptions, out Epic.OnlineServices.Lobby.Attribute? outAttribute);
                if (copyAttrResult == Result.Success && outAttribute != null && outAttribute?.Data != null){
                    LobbyAttribute attr = EOSLobbyExtenstions.GenerateLobbyAttribute(outAttribute);
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
                    Result memberAttributeResult = outLobbyDetailsHandle.CopyMemberAttributeByIndex(ref lobbyDetailsCopyMemberAttributeByIndexOptions, out Epic.OnlineServices.Lobby.Attribute? outAttribute);

                    if (memberAttributeResult != Result.Success){
                        Debug.LogFormat("Lobbies (InitFromLobbyDetails): can't copy member attribute. Error code: {0}", memberAttributeResult);
                        continue;
                    }

                    LobbyAttribute newAttribute = EOSLobbyExtenstions.GenerateLobbyAttribute(outAttribute);
 
                    Members[memberIndex].MemberAttributes.Add(newAttribute.Key, newAttribute);
                }
            }
        }
    }
    /// <summary>
    /// Class represents all Lobby Member properties
    /// </summary>
    public class LobbyMember {
        public ProductUserId ProductId;
        public Dictionary<string, LobbyAttribute> MemberAttributes = new Dictionary<string, LobbyAttribute>();
    }
    /// <summary>
    /// Class represents all Lobby Attribute properties
    /// </summary>
    public class LobbyAttribute {
        public LobbyAttributeVisibility Visibility = LobbyAttributeVisibility.Public;
        
        public string Key;
        //Only one of the following properties will have valid data (depending on 'ValueType')
        public bool? BOOLEAN { get; private set; }
        public int? INT64 { get; private set; } = 0;
        public double? DOUBLE { get; private set; } = 0.0;
        public string STRING { get; private set; }
        public AttributeType ValueType { get; private set; } = AttributeType.String;
        public ComparisonOp comparisonOption = ComparisonOp.Equal;
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
}