using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using PlayEveryWare.EpicOnlineServices;
using SynicSugar.P2P;
using UnityEngine;
using UnityEngine.Events;

namespace SynicSugar.MatchMake {
    public class MatchMakeManager : MonoBehaviour {
#region Singleton
        private MatchMakeManager(){}
        public static MatchMakeManager Instance { get; private set; }
        void Awake() {
            if( Instance != null ) {
                Destroy( this.gameObject );
                return;
            }
            Instance = this;
            DontDestroyOnLoad(this);

            eosLobby = new EOSLobby(maxSearchResult, TimeoutSec);
            MemberUpdatedNotifier = new();

            if(lobbyIdSaveType == RecconectLobbyIdSaveType.CustomMethod){
                if(customSaveLobbyID != null && customDeleteLobbyID != null){
                    lobbyIDMethod.Save += () => customSaveLobbyID.Invoke();
                    lobbyIDMethod.Delete += () => customDeleteLobbyID.Invoke();
                }
            }
            localUserId = UserId.GetUserId(EOSManager.Instance.GetProductUserId());
        }
        void OnDestroy() {
            if( Instance == this ) {
                //For sub events
                lobbyIDMethod.Clear();
                asyncLobbyIDMethod.Clear();
                MemberUpdatedNotifier.Clear();

                Instance = null;
            }
        }
#endregion
        //Option
        public uint maxSearchResult = 5;

        [HideInInspector] public int hostsTimeoutSec = 180;
        [Range(20f, 600f)]
        public int TimeoutSec = 180;
    #region TODO: Change this to Enum and display only one field for the selected way on UnityEditor.
        public enum RecconectLobbyIdSaveType {
            NoReconnection, Playerprefs, CustomMethod, AsyncCustomMethod
        }
        /// <summary>
        /// AsyncCustomMethod is under Experimental.
        /// </summary>
        [Header("AsyncCustomMethod is under Experimental")]
        public RecconectLobbyIdSaveType lobbyIdSaveType = RecconectLobbyIdSaveType.Playerprefs;
        [Header("PlayerPrefs")]
        /// <summary>
        /// Key to hold LobbyID after MatchMake. 
        /// </summary>
        public string playerprefsSaveKey = "eos_lobbyid";
        [Header("CustomMetod")]
        [SerializeField] UnityEvent customSaveLobbyID;
        [SerializeField] UnityEvent customDeleteLobbyID;
    #endregion
    #region SaveEvent's
        public LobbyIDMethod lobbyIDMethod = new LobbyIDMethod();
        public AsyncLobbyIDMethod asyncLobbyIDMethod = new AsyncLobbyIDMethod();
    #endregion
        internal EOSLobby eosLobby { get; private set; }
        internal CancellationTokenSource matchingToken;
        public MatchMakingGUIEvents MatchMakingGUIEvents = new MatchMakingGUIEvents();
        // Events
        public MemberUpdatedNotifier MemberUpdatedNotifier;
        /// <summary>
        /// If having error, this value is changed. If Success, this remains Result.None.
        /// </summary>
        public Result LastResultCode { get; internal set; } = Result.None;
        /// <summary>
        /// Is this user Host?
        /// </summary>
        public bool isHost { get { return eosLobby.CurrentLobby.isHost(); }}
        UserId localUserId;
        /// <summary>
        /// Is this id is LocalUser's id?
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public bool isLocalUserId(UserId id){
            return id == localUserId;
        }
        /// <summary>
        /// Is this id is LocalUser's id?
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public bool isLocalUserId(string id){
            return UserId.GetUserId(id) == localUserId;
        }
        public int GetCurrentLobbyMemberCount(){
           return eosLobby.GetCurrentLobbyMemberCount();
        }

        public int GetMaxLobbyMemberCount(){
           return eosLobby.GetMaxLobbyMemberCount();
        }

        /// <summary>
        /// MatchMake player with conditions and get the data for p2p connect. <br />
        /// Search a lobby, then if can't join, create a lobby as host. When lobby filled with max members, the lobby is closed automatically.
        /// </summary>
        /// <param name="lobbyCondition">Crate by EOSLobbyExtenstions.GenerateLobby().</param>
        /// <param name="token">For cancel matchmaking. This is used by CancelCurrentMatchMake.
        /// If pass, we implement OperationCanceledException by ourself.
        /// If not pass, such processe are done internally and return false when we cancel matchmake.</param>
        /// <param name="userAttributes">The user attributes of names, job and so on that is needed before P2P. <br />
        /// These should be used just for matchmaking and the kick, the data for actual game should be exchanged via p2p for the lag and server bandwidth .</param>
        /// <returns></returns>
        public async UniTask<bool> SearchAndCreateLobby(Lobby lobbyCondition, List<AttributeData> userAttributes = null, CancellationTokenSource token = default(CancellationTokenSource)){
            //This task can be canceled from outside even if pass no token.
            //So need try-catch
            bool useTryCatch = token == default;
            matchingToken = token == default ? new CancellationTokenSource() : token;
            
            bool canMatch = false;

            //Match at Lobby
            if(useTryCatch){
                try{
                    canMatch = await eosLobby.StartMatching(lobbyCondition, matchingToken.Token, userAttributes, 0);
                }catch(OperationCanceledException){
                #if SYNICSUGAR_LOG
                    Debug.Log("MatchMaking is canceled");
                #endif
                    MatchMakingGUIEvents.ChangeState(MatchMakingGUIEvents.State.Standby);
                    return false;
                }
            }else{
                canMatch = await eosLobby.StartMatching(lobbyCondition, matchingToken.Token, userAttributes, 0);
            }

            if(!canMatch){
                MatchMakingGUIEvents.ChangeState(MatchMakingGUIEvents.State.Standby);
                return false;
            }
            
            MatchMakingGUIEvents.ChangeState(MatchMakingGUIEvents.State.Ready);
            return true;
        }
        /// <summary>
        /// MatchMake player with conditions and get the data for p2p connect. <br />
        /// Search a lobby, then if can't join, create a lobby as host. After lobby filled with min members, Host closes lobby with FinishMatchmaking().
        /// </summary>
        /// <param name="lobbyCondition">Crate by EOSLobbyExtenstions.GenerateLobby().</param>
        /// <param name="token">For cancel matchmaking. This is used by CancelCurrentMatchMake.
        /// If pass, we implement OperationCanceledException by ourself.
        /// If not pass, such processe are done internally and return false when we cancel matchmake.</param>
        /// <param name="minLobbyMember">Minimum number of lobby members required. <br />
        /// To close automatically, 0 or pass nothing. The case completes matchmaking on filled in lobby by max members. <br />
        /// If 2 or more, after lobby reach this min value,  (If we use EnableManualFinish event.) ManualFinish Button is displayed for Host. Host calls FinishMatchmake(), then the matchmaking is completed and start p2p. (If not call FinishMatchmake(), the matchmaking is going on until timeout and get failed.)</param>
        /// <param name="userAttributes">The user attributes of names, job and so on that is needed before P2P. <br />
        /// These should be used just for matchmaking and the kick, the data for actual game should be exchanged via p2p for the lag and server bandwidth .</param>
        /// <returns></returns>
        public async UniTask<bool> SearchAndCreateLobby(Lobby lobbyCondition, uint minLobbyMember, List<AttributeData> userAttributes = null, CancellationTokenSource token = default(CancellationTokenSource)){  
            //This task can be canceled from outside even if pass no token.
            //So need try-catch
            bool useTryCatch = token == default;
            matchingToken = token == default ? new CancellationTokenSource() : token;

            if(minLobbyMember < 2 || minLobbyMember < lobbyCondition.MaxLobbyMembers){
                minLobbyMember = 0;
            }
            
            bool canMatch = false;
            //Match at Lobby
            if(useTryCatch){
                try{
                    canMatch = await eosLobby.StartMatching(lobbyCondition, matchingToken.Token, userAttributes, minLobbyMember);
                }catch(OperationCanceledException){
                #if SYNICSUGAR_LOG
                    Debug.Log("MatchMaking is canceled");
                #endif
                    MatchMakingGUIEvents.ChangeState(MatchMakingGUIEvents.State.Standby);
                    return false;
                }
            }else{
                canMatch = await eosLobby.StartMatching(lobbyCondition, matchingToken.Token, userAttributes, minLobbyMember);
            }

            if(!canMatch){
                MatchMakingGUIEvents.ChangeState(MatchMakingGUIEvents.State.Standby);
                return false;
            }
            
            MatchMakingGUIEvents.ChangeState(MatchMakingGUIEvents.State.Ready);
            return true;
        }
        /// <summary>
        /// Search lobby to join, then get the data for p2p connect. <br />
        /// Recommend: SearchAndCreateLobby()
        /// </summary>
        /// <param name="lobbyCondition">Crate by EOSLobbyExtenstions.GenerateLobby().</param>
        /// <param name="token">For cancel matchmaking. This is used by CancelCurrentMatchMake.
        /// If pass, we implement OperationCanceledException by ourself.
        /// If not pass, such processe are done internally and return false when we cancel matchmake.</param>
        /// <param name="userAttributes">The user attributes of names, job and so on that is needed before P2P. <br />
        /// These should be used just for matchmaking and the kick, the data for actual game should be exchanged via p2p for the lag and server bandwidth .</param>
        /// <returns></returns>
        public async UniTask<bool> SearchLobby(Lobby lobbyCondition, List<AttributeData> userAttributes = null, CancellationTokenSource token = default(CancellationTokenSource)){
            bool useTryCatch = token == default;
            matchingToken = useTryCatch ? new CancellationTokenSource() : token;
            
            bool canMatch = false;
            //Match at Lobby
            if(useTryCatch){
                try{
                    canMatch = await eosLobby.StartJustSearch(lobbyCondition, matchingToken.Token, userAttributes);
                }catch(OperationCanceledException){
                #if SYNICSUGAR_LOG
                    Debug.Log("MatchMaking is canceled");
                #endif
                    MatchMakingGUIEvents.ChangeState(MatchMakingGUIEvents.State.Standby);
                    return false;
                }
            }else{
                canMatch = await eosLobby.StartJustSearch(lobbyCondition, matchingToken.Token, userAttributes);
            }
            

            if(!canMatch){
                MatchMakingGUIEvents.ChangeState(MatchMakingGUIEvents.State.Standby);
                return false;
            }
            
            MatchMakingGUIEvents.ChangeState(MatchMakingGUIEvents.State.Ready);
            return true;
        }
        
        /// <summary>
        /// Create lobby and wait for other users, then get the data for p2p connect. <br />
        /// Recommend: SearchAndCreateLobby()
        /// </summary>
        /// <param name="lobbyCondition">Crate by EOSLobbyExtenstions.GenerateLobby().</param>
        /// <param name="token">For cancel matchmaking. This is used by CancelCurrentMatchMake.
        /// If pass, we implement OperationCanceledException by ourself.
        /// If not pass, such processe are done internally and return false when we cancel matchmake.</param>
        /// <param name="userAttributes">The user attributes of names, job and so on that is needed before P2P. <br />
        /// These should be used just for matchmaking and the kick, the data for actual game should be exchanged via p2p for the lag and server bandwidth .</param>
        /// <returns></returns>
        public async UniTask<bool> CreateLobby(Lobby lobbyCondition, List<AttributeData> userAttributes = null, CancellationTokenSource token = default(CancellationTokenSource)){
            bool useTryCatch = token == default;
            matchingToken = useTryCatch ? new CancellationTokenSource() : token;
            
            bool canMatch = false;
            //Match at Lobby
            if(useTryCatch){
                try{
                    canMatch = await eosLobby.StartJustCreate(lobbyCondition, matchingToken.Token, userAttributes, 0);;
                }catch(OperationCanceledException){
                #if SYNICSUGAR_LOG
                    Debug.Log("MatchMaking is canceled");
                #endif
                    MatchMakingGUIEvents.ChangeState(MatchMakingGUIEvents.State.Standby);
                    return false;
                }
            }else{
                canMatch = await eosLobby.StartJustCreate(lobbyCondition, matchingToken.Token, userAttributes, 0);
            }
            
            if(!canMatch){
                MatchMakingGUIEvents.ChangeState(MatchMakingGUIEvents.State.Standby);
                return false;
            }
            
            MatchMakingGUIEvents.ChangeState(MatchMakingGUIEvents.State.Ready);
            return true;
        }
        /// <summary>
        /// Create lobby and wait for other users, then get the data for p2p connect. <br />
        /// Recommend: SearchAndCreateLobby()
        /// </summary>
        /// <param name="lobbyCondition">Crate by EOSLobbyExtenstions.GenerateLobby().</param>
        /// <param name="token">For cancel matchmaking. This is used by CancelCurrentMatchMake.
        /// If pass, we implement OperationCanceledException by ourself.
        /// If not pass, such processe are done internally and return false when we cancel matchmake.</param>
        /// <param name="minLobbyMember">Minimum number of lobby members required. <br />
        /// To close automatically, 0 or pass nothing. The case completes matchmaking on filled in lobby by max members. <br />
        /// If 2 or more, after lobby reach this min value,  (If we use EnableManualFinish event.) ManualFinish Button is displayed for Host. Host calls FinishMatchmake(), then the matchmaking is completed and start p2p. (If not call FinishMatchmake(), the matchmaking is going on until timeout and get failed.)</param>
        /// <param name="userAttributes">The user attributes of names, job and so on that is needed before P2P. <br />
        /// These should be used just for matchmaking and the kick, the data for actual game should be exchanged via p2p for the lag and server bandwidth .</param>
        /// <returns></returns>
        public async UniTask<bool> CreateLobby(Lobby lobbyCondition, uint minLobbyMember, List<AttributeData> userAttributes = null, CancellationTokenSource token = default(CancellationTokenSource)){
            bool useTryCatch = token == default;
            matchingToken = useTryCatch ? new CancellationTokenSource() : token;
            
            if(minLobbyMember < 2 || minLobbyMember < lobbyCondition.MaxLobbyMembers){
                minLobbyMember = 0;
            }
            
            bool canMatch = false;
            //Match at Lobby
            if(useTryCatch){
                try{
                    canMatch = await eosLobby.StartJustCreate(lobbyCondition, matchingToken.Token, userAttributes, 0);;
                }catch(OperationCanceledException){
                #if SYNICSUGAR_LOG
                    Debug.Log("MatchMaking is canceled");
                #endif
                    MatchMakingGUIEvents.ChangeState(MatchMakingGUIEvents.State.Standby);
                    return false;
                }
            }else{
                canMatch = await eosLobby.StartJustCreate(lobbyCondition, matchingToken.Token, userAttributes, 0);
            }
            
            if(!canMatch){
                MatchMakingGUIEvents.ChangeState(MatchMakingGUIEvents.State.Standby);
                return false;
            }
            
            MatchMakingGUIEvents.ChangeState(MatchMakingGUIEvents.State.Ready);
            return true;
        }
        /// <summary>
        /// Join the Lobby with saved LobbyID. <br />
        /// Call this at the start of game or match-make.
        /// </summary>
        /// <param name="LobbyID">Lobby ID to <c>re</c>-connect</param>
        public async UniTask<bool> ReconnectLobby(string LobbyID, CancellationTokenSource token = default(CancellationTokenSource)){
            if(string.IsNullOrEmpty(LobbyID)){
                return false;
            }
    #if SYNICSUGAR_LOG
            Debug.Log($"Try Recconect with {LobbyID}");
    #endif
            matchingToken = token == default ? new CancellationTokenSource() : token;
            
            bool canJoin =  await eosLobby.JoinLobbyBySavedLobbyId(LobbyID, matchingToken.Token);

            if(!canJoin){
                MatchMakingGUIEvents.ChangeState(MatchMakingGUIEvents.State.Standby);
                return false;
            }

            MatchMakingGUIEvents.ChangeState(MatchMakingGUIEvents.State.Ready);
            return true;
        }
        /// <summary>
        /// Exit lobby and cancel MatchMake.
        /// </summary>
        /// <param name="token">token for this task</param>
        /// <param name="removeManager">If true, destroy ConnectManager. When we move to the other scene (where we don't need ConnectManager) after this, we should pass true.</param>
        /// <returns></returns>
        public async UniTask<bool> CancelCurrentMatchMake(bool removeManager = false, CancellationToken token = default(CancellationToken)){
            if(matchingToken == null || !matchingToken.Token.CanBeCanceled){
            #if SYNICSUGAR_LOG
                Debug.Log("CancelCurrentMatchMake: Is this user currently in matchmaking?");
            #endif
                return false;
            }
            bool canCancel = await eosLobby.CancelMatchMaking(matchingToken, token);
            
            if(removeManager && canCancel){
                Destroy(this.gameObject);
            }
            return canCancel;
        }
        /// <summary>
        /// Host kicks a specific target from Lobby. Only one tareget can be kicked at a time.
        /// </summary>
        /// <param name="token">token for this task</param>
        /// <returns></returns>
        public async UniTask<bool> KickTargetFromLobby(UserId targetId, CancellationToken token = default(CancellationToken)){
            LastResultCode = Result.None;
            token = token == default ? this.GetCancellationTokenOnDestroy() : token;
            bool canKick = await eosLobby.KickTargetMember(targetId, token);

            return canKick;
        }
        /// <summary>
        /// Leave the current lobby in Game.
        /// </summary>
        /// <param name="token"></param>
        internal async UniTask<bool> ExitCurrentLobby(CancellationToken token){
            LastResultCode = Result.None;
            bool canDestroy = await eosLobby.LeaveLobby(false, token);

            return canDestroy;
        }
        /// <summary>
        /// Destroy the current lobby on the end of Game.
        /// </summary>
        /// <param name="token"></param>
        /// <returns>True on success. If user isn't host, return false.</returns>
        internal async UniTask<bool> CloseCurrentLobby(CancellationToken token){
            bool canDestroy = await eosLobby.DestroyLobby(token);

            return canDestroy;
        }
        /// <summary>
        /// Get Last ERROR Result code. None means init state. 
        /// </summary>
        /// <returns>-3 - -1 is unique code of SynicSugar. Others is same with Epic's result code.</returns>
        public Result GetLastErrorCode(){
            return LastResultCode;
        }
    #region LobbyID
        /// <summary>
        /// Get ID of the current lobby that a user's participating
        /// </summary>
        /// <returns>string LobbyID</returns>
        public string GetCurrentLobbyID(){
            return eosLobby.GetCurrentLobbyID();
        }
        
        /// <summary>
        /// Save lobby data for player to connect unexpectedly left lobby like power off.
        /// </summary>
        internal async UniTask OnSaveLobbyID(){
    #if SYNICSUGAR_LOG
            Debug.Log($"Save LobbyID by {lobbyIdSaveType}");
    #endif
            switch(lobbyIdSaveType){
                case RecconectLobbyIdSaveType.NoReconnection:
                return;
                case RecconectLobbyIdSaveType.Playerprefs:
                    PlayerPrefs.SetString(playerprefsSaveKey, GetCurrentLobbyID());
                return;
                case RecconectLobbyIdSaveType.CustomMethod:
                    lobbyIDMethod.OnSave();
                return;
                case RecconectLobbyIdSaveType.AsyncCustomMethod:
                    await asyncLobbyIDMethod.OnSave();
                return;
            }
        }
        /// <summary>
        /// Delete save data for player not to connect the current lobby after the battle.
        /// </summary>
        internal async UniTask OnDeleteLobbyID(){
    #if SYNICSUGAR_LOG
            Debug.Log($"Delete LobbyID by {lobbyIdSaveType}");
    #endif
            switch(lobbyIdSaveType){
                case RecconectLobbyIdSaveType.NoReconnection:
                return;
                case RecconectLobbyIdSaveType.Playerprefs:
                    PlayerPrefs.DeleteKey(playerprefsSaveKey);
                return;
                case RecconectLobbyIdSaveType.CustomMethod:
                    lobbyIDMethod.OnSave();
                return;
                case RecconectLobbyIdSaveType.AsyncCustomMethod:
                    await asyncLobbyIDMethod.OnDelete();
                return;
            }
        }
        /// <summary>
        /// For the DEFAULT way, get LobbyID from PlayerPrefs.<br />
        /// NOTE: To use custom method about LobbyID, need get ID from that location.
        /// </summary>
        /// <returns>If it exists, returns STRING key. If not, returns String.Empty.</returns>
        public string GetReconnectLobbyID(){
            return PlayerPrefs.GetString (playerprefsSaveKey, System.String.Empty);
        }
    #endregion
        /// <summary>
        /// For search conditions.<br />
        /// About attributes, use GenerateLobbyAttribute to set.
        /// </summary>
        /// <param name="bucket">important condition like game-mode, region, map</param>
        /// <param name="MaxPlayers">Max number of user allowed in the lobby</param>
        /// <returns></returns>
        public static Lobby GenerateLobbyObject(string[] bucket, uint MaxPlayers = 2, bool useVoiceChat = false){
            Lobby lobby = new Lobby();
            lobby.SetBucketID(bucket);
            lobby.MaxLobbyMembers = MaxPlayers;
            lobby.bEnableRTCRoom = useVoiceChat;

            return lobby;
        }
        /// <summary>
        /// Get target attribute about Key.
        /// </summary>
        /// <param name="target">target user id</param>
        /// <param name="Key">attribute key</param>
        /// <returns>If no data, return null.</returns>
        public AttributeData GetTargetAttributeData(UserId target, string Key){
            return eosLobby.CurrentLobby.Members[target.ToString()]?.GetAttributeData(Key);
        }
        /// <summary>
        /// Get target all attributes.
        /// </summary>
        /// <param name="target">target user id</param>
        /// <returns>If no data, return null.</returns>
        public List<AttributeData> GetTargetAttributeData(UserId target){
            return eosLobby.CurrentLobby.Members[target.ToString()]?.Attributes;
        }
    }
}
