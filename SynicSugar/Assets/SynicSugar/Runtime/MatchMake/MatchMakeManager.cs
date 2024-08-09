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
        public static MatchMakeManager Instance { get; private set; }
        void Awake() {
            if( Instance != null ) {
                Destroy( this.gameObject );
                return;
            }
            Instance = this;
            DontDestroyOnLoad(this);

            eosLobby = new EOSLobby(maxSearchResult, TimeoutSec, P2PSetupTimeoutSec);
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
                MatchMakingGUIEvents.Clear();

                Instance = null;
            }
        }
#endregion
        //Option
        [Range(1, 100)]
        public uint maxSearchResult = 5;

        /// <summary>
        /// This time is from the start of matchmaking until the the end of matchmaking(= just before preparation for p2p connect).<br />
        /// If that time passes before users start p2p setup, the matchmaking APIs return false as Timeout.<br />
        /// When we need the more time than 10 minutes for timeout, we can set TimeoutSec directly.<br />
        /// If call SetTimeoutSec after matchmaking has started could cause bugs, so set this in the Editor or call SetTimeoutSec before matchmaking.
        /// </summary>
        [Range(20, 600)]
        public ushort TimeoutSec = 180;
        /// <summary>This time is from the start of preparation for p2p until the the end of the preparetion.<br />
        // If that time passes before matchmaking APIs return result, the matchmaking APIs return false as Timeout.<br />
        // When we need the more time than 1 minutes for timeout, we can set TimeoutSec directly.<br />
        /// If call SetTimeoutSec after matchmaking has started could cause bugs, so set this in the Editor or call SetTimeoutSec before matchmaking.
        /// </summary>
        [Range(5, 60)]
        public ushort P2PSetupTimeoutSec = 15;


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
        /// Is this user Host?
        /// </summary>
        public bool isHost { get { return eosLobby.CurrentLobby.isHost(); }}
        UserId localUserId;
        /// <summary>
        /// Is this id is LocalUser's id?
        /// </summary>
        /// <param name="id"></param>
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
           return eosLobby.GetLobbyMemberLimit();
        }
        
        /// <summary>
        /// Set Timeout sec. Should call this before start matchmake. <br />
        /// If use this, need call this before start matchmaking.
        /// </summary>
        /// <param name="MatchmakingTimeout">Timeout sec from the start of matchmaking until the the end of matchmaking(= just before preparation for p2p connect).<br/>
        /// If nothing is passed, pass the value set in Editor.　Recommend:30-300</param>
        /// <param name="InitialConnectionTimeout">Timeout sec from the start of preparation for p2p until the the end of the preparetion. <br />
        /// If nothing is passed, pass the value set in Editor.　Recommend:5-20</param>
        public void SetTimeoutSec(ushort MatchmakingTimeout = 180, ushort InitialConnectionTimeout = 15){
            eosLobby.SetTimeoutSec(MatchmakingTimeout == 180 ? TimeoutSec : MatchmakingTimeout, InitialConnectionTimeout == 15 ? P2PSetupTimeoutSec : InitialConnectionTimeout);
        }
        /// <summary>
        /// MatchMake player with conditions and get the data for p2p connect. <br />
        /// Search a lobby, then if can't join, create a lobby as host. When lobby filled with max members, the lobby is closed automatically.
        /// </summary>
        /// <param name="lobbyCondition">Crate by EOSLobbyExtenstions.GenerateLobby().</param>
        /// <param name="token">For cancel matchmaking. This is used by CancelCurrentMatchMake.
        /// If pass, we implement OperationCanceledException by ourself.
        /// If not pass, such processe are done internally and return false when we cancel matchmake.</param>
        /// <returns></returns>
        public async UniTask<Result> SearchAndCreateLobby(Lobby lobbyCondition, CancellationTokenSource token = default(CancellationTokenSource)){
            //This task can be canceled from outside even if pass no token.
            //So need try-catch
            bool needTryCatch = token == default;
            matchingToken = needTryCatch ? new CancellationTokenSource() : token;
            
            Result canMatch = Result.None;

            //Match at Lobby
            if(needTryCatch){
                try{
                    canMatch = await eosLobby.StartMatching(lobbyCondition, matchingToken.Token, new(), 0);
                }catch(OperationCanceledException){
                #if SYNICSUGAR_LOG
                    Debug.Log("MatchMaking is canceled");
                #endif
                    MatchMakingGUIEvents.ChangeState(MatchMakingGUIEvents.State.Standby);
                    return Result.Canceled;
                }
            }else{
                canMatch = await eosLobby.StartMatching(lobbyCondition, matchingToken.Token, new(), 0);
            }

            if(canMatch != Result.Success){
                MatchMakingGUIEvents.ChangeState(MatchMakingGUIEvents.State.Standby);
                return canMatch;
            }
            
            MatchMakingGUIEvents.ChangeState(MatchMakingGUIEvents.State.Ready);
            return Result.Success;
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
        public async UniTask<Result> SearchAndCreateLobby(Lobby lobbyCondition, uint minLobbyMember, List<AttributeData> userAttributes = null, CancellationTokenSource token = default(CancellationTokenSource)){  
            //This task can be canceled from outside even if pass no token.
            //So need try-catch
            bool needTryCatch = token == default;
            matchingToken = needTryCatch ? new CancellationTokenSource() : token;

            if(minLobbyMember < 2 || minLobbyMember > lobbyCondition.MaxLobbyMembers){
                minLobbyMember = 0;
            }
            
            Result canMatch = Result.None;
            //Match at Lobby
            if(needTryCatch){
                try{
                    canMatch = await eosLobby.StartMatching(lobbyCondition, matchingToken.Token, userAttributes ?? new(), minLobbyMember);
                }catch(OperationCanceledException){
                #if SYNICSUGAR_LOG
                    Debug.Log("MatchMaking is canceled");
                #endif
                    MatchMakingGUIEvents.ChangeState(MatchMakingGUIEvents.State.Standby);
                    return Result.Canceled;
                }
            }else{
                canMatch = await eosLobby.StartMatching(lobbyCondition, matchingToken.Token, userAttributes ?? new(), minLobbyMember);
            }

            if(canMatch != Result.Success){
                MatchMakingGUIEvents.ChangeState(MatchMakingGUIEvents.State.Standby);
                return canMatch;
            }
            
            MatchMakingGUIEvents.ChangeState(MatchMakingGUIEvents.State.Ready);
            return Result.Success;
        }
        /// <summary>
        /// Search lobby to join, then get the data for p2p connect. <br />
        /// Recommend: SearchAndCreateLobby()
        /// </summary>
        /// <param name="lobbyCondition">Crate by EOSLobbyExtenstions.GenerateLobby().</param>
        /// <param name="token">For cancel matchmaking. This is used by CancelCurrentMatchMake.
        /// If pass, we implement OperationCanceledException by ourself.
        /// If not pass, such processe are done internally and return false when we cancel matchmake.</param>
        /// <returns></returns>
        public async UniTask<Result> SearchLobby(Lobby lobbyCondition, CancellationTokenSource token = default(CancellationTokenSource)){
            bool needTryCatch = token == default;
            matchingToken = needTryCatch ? new CancellationTokenSource() : token;
            
            Result canMatch = Result.None;
            //Match at Lobby
            if(needTryCatch){
                try{
                    canMatch = await eosLobby.StartJustSearch(lobbyCondition, matchingToken.Token, new(), 0);
                }catch(OperationCanceledException){
                #if SYNICSUGAR_LOG
                    Debug.Log("MatchMaking is canceled");
                #endif
                    MatchMakingGUIEvents.ChangeState(MatchMakingGUIEvents.State.Standby);
                    return Result.Canceled;
                }
            }else{
                canMatch = await eosLobby.StartJustSearch(lobbyCondition, matchingToken.Token, null, 0);
            }
            

            if(canMatch != Result.Success){
                MatchMakingGUIEvents.ChangeState(MatchMakingGUIEvents.State.Standby);
                return canMatch;
            }
            
            MatchMakingGUIEvents.ChangeState(MatchMakingGUIEvents.State.Ready);
            return Result.Success;
        }
        /// <summary>
        /// Search lobby to join, then get the data for p2p connect. <br />
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
        public async UniTask<Result> SearchLobby(Lobby lobbyCondition, uint minLobbyMember, List<AttributeData> userAttributes = null, CancellationTokenSource token = default(CancellationTokenSource)){
            bool needTryCatch = token == default;
            matchingToken = needTryCatch ? new CancellationTokenSource() : token;

            if(minLobbyMember < 2 || minLobbyMember > lobbyCondition.MaxLobbyMembers){
                minLobbyMember = 0;
            }
            
            Result canMatch = Result.None;
            //Match at Lobby
            if(needTryCatch){
                try{
                    canMatch = await eosLobby.StartJustSearch(lobbyCondition, matchingToken.Token, userAttributes ?? new(), minLobbyMember);
                }catch(OperationCanceledException){
                #if SYNICSUGAR_LOG
                    Debug.Log("MatchMaking is canceled");
                #endif
                    MatchMakingGUIEvents.ChangeState(MatchMakingGUIEvents.State.Standby);
                    return Result.Canceled;
                }
            }else{
                canMatch = await eosLobby.StartJustSearch(lobbyCondition, matchingToken.Token, userAttributes ?? new(), minLobbyMember);
            }
            

            if(canMatch != Result.Success){
                MatchMakingGUIEvents.ChangeState(MatchMakingGUIEvents.State.Standby);
                return canMatch;
            }
            
            MatchMakingGUIEvents.ChangeState(MatchMakingGUIEvents.State.Ready);
            return Result.Success;
        }
        
        /// <summary>
        /// Create lobby and wait for other users, then get the data for p2p connect. <br />
        /// Recommend: SearchAndCreateLobby()
        /// </summary>
        /// <param name="lobbyCondition">Crate by EOSLobbyExtenstions.GenerateLobby().</param>
        /// <param name="token">For cancel matchmaking. This is used by CancelCurrentMatchMake.
        /// If pass, we implement OperationCanceledException by ourself.
        /// If not pass, such processe are done internally and return false when we cancel matchmake.</param>
        /// <returns></returns>
        public async UniTask<Result> CreateLobby(Lobby lobbyCondition, CancellationTokenSource token = default(CancellationTokenSource)){
            bool needTryCatch = token == default;
            matchingToken = needTryCatch ? new CancellationTokenSource() : token;
            
            Result canMatch = Result.None;
            //Match at Lobby
            if(needTryCatch){
                try{
                    canMatch = await eosLobby.StartJustCreate(lobbyCondition, matchingToken.Token, new(), 0);
                }catch(OperationCanceledException){
                #if SYNICSUGAR_LOG
                    Debug.Log("MatchMaking is canceled");
                #endif
                    MatchMakingGUIEvents.ChangeState(MatchMakingGUIEvents.State.Standby);
                    return Result.Canceled;
                }
            }else{
                canMatch = await eosLobby.StartJustCreate(lobbyCondition, matchingToken.Token, new(), 0);
            }
            
            if(canMatch != Result.Success){
                MatchMakingGUIEvents.ChangeState(MatchMakingGUIEvents.State.Standby);
                return canMatch;
            }
            
            MatchMakingGUIEvents.ChangeState(MatchMakingGUIEvents.State.Ready);
            return Result.Success;
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
        public async UniTask<Result> CreateLobby(Lobby lobbyCondition, uint minLobbyMember, List<AttributeData> userAttributes = null, CancellationTokenSource token = default(CancellationTokenSource)){
            bool needTryCatch = token == default;
            matchingToken = needTryCatch ? new CancellationTokenSource() : token;
            
            if(minLobbyMember < 2 || minLobbyMember > lobbyCondition.MaxLobbyMembers){
                minLobbyMember = 0;
            }
            
            Result canMatch = Result.Success;
            //Match at Lobby
            if(needTryCatch){
                try{
                    canMatch = await eosLobby.StartJustCreate(lobbyCondition, matchingToken.Token, userAttributes ?? new(), minLobbyMember);
                }catch(OperationCanceledException){
                #if SYNICSUGAR_LOG
                    Debug.Log("MatchMaking is canceled");
                #endif
                    MatchMakingGUIEvents.ChangeState(MatchMakingGUIEvents.State.Standby);
                    return Result.Canceled;
                }
            }else{
                canMatch = await eosLobby.StartJustCreate(lobbyCondition, matchingToken.Token, userAttributes ?? new(), minLobbyMember);
            }
            
            if(canMatch != Result.Success){
                MatchMakingGUIEvents.ChangeState(MatchMakingGUIEvents.State.Standby);
                return canMatch;
            }

            MatchMakingGUIEvents.ChangeState(MatchMakingGUIEvents.State.Ready);
            return Result.Success;
        }
        bool isConcluding;
        /// <summary>
        /// After the requirement is met, Host manually finish matchmaking and start p2p.
        /// </summary>
        public void ConcludeMatchMake(){
            if(isConcluding || !isHost){
                return;
            }
            isConcluding = true;
            eosLobby.SwitchLobbyAttribute();
            isConcluding = false;
        }
        /// <summary>
        /// Join the Lobby with saved LobbyID. <br />
        /// Call this at the start of game or match-make.
        /// </summary>
        /// <param name="LobbyID">Lobby ID to <c>re</c>-connect</param>
        public async UniTask<Result> ReconnectLobby(string LobbyID, CancellationTokenSource token = default(CancellationTokenSource)){
            if(string.IsNullOrEmpty(LobbyID)){
                return Result.InvalidParameters;
            }
    #if SYNICSUGAR_LOG
            Debug.Log($"Try Recconect with {LobbyID}");
    #endif
            matchingToken = token == default ? new CancellationTokenSource() : token;
            
            Result canJoin = await eosLobby.JoinLobbyBySavedLobbyId(LobbyID, matchingToken.Token);

            if(canJoin != Result.Success){
                MatchMakingGUIEvents.ChangeState(MatchMakingGUIEvents.State.Standby);
                return canJoin;
            }

            MatchMakingGUIEvents.ChangeState(MatchMakingGUIEvents.State.Ready);
            return Result.Success;
        }
        
        /// <summary>
        /// Exit lobby and cancel MatchMake. <br />
        /// When Host calls this method, Guest will becomes new Host automatically after Host-migration.
        /// </summary>
        /// <param name="token">token for this task</param>
        /// <param name="destroyManager">If true, destroy NetworkManager after cancel matchmake.</param>
        /// <returns></returns>
        public async UniTask<Result> ExitCurrentMatchMake(bool destroyManager = true, CancellationToken token = default(CancellationToken)){
            if(matchingToken == null || !matchingToken.Token.CanBeCanceled){
            #if SYNICSUGAR_LOG
                Debug.Log("ExitCurrentMatchMake: Is this user currently in matchmaking?");
            #endif
                return Result.InvalidAPICall;
            }
            Result canCancel = await eosLobby.CancelMatchMaking(matchingToken, token);
            
            if(destroyManager && canCancel == Result.Success){
                Destroy(this.gameObject);
            }
            return canCancel;
        }
        /// <summary>
        /// If Host, destroy lobby and cancels MatchMake.<br />
        /// If Guest, just leave lobby and cancels MatchMake.<br />
        /// We use ConnectHub.Instance.ExitSession and ConnectHub.Instance.CloseSession after matchmaking.
        /// </summary>
        /// <param name="token">token for this task</param>
        /// <param name="destroyManager">If true, destroy NetworkManager after cancel matchmake.</param>
        /// <returns></returns>
        public async UniTask<Result> CloseCurrentMatchMake(bool destroyManager = true, CancellationToken token = default(CancellationToken)){
            if(matchingToken == null || !matchingToken.Token.CanBeCanceled){
            #if SYNICSUGAR_LOG
                Debug.Log("CloseCurrentMatchMake: Is this user currently in matchmaking?");
            #endif
                return Result.InvalidAPICall;
            }
            Result canCancel = await eosLobby.CloseMatchMaking(matchingToken, token);
            
            if(destroyManager && canCancel == Result.Success){
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
            token = token == default ? this.GetCancellationTokenOnDestroy() : token;
            bool canKick = await eosLobby.KickTargetMember(targetId, token);

            return canKick;
        }
        /// <summary>
        /// Leave the current lobby in Game.
        /// </summary>
        /// <param name="token"></param>
        internal async UniTask<Result> ExitCurrentLobby(CancellationToken token){
            Result canDestroy = await eosLobby.LeaveLobby(false, token);

            return canDestroy;
        }
        /// <summary>
        /// Destroy the current lobby on the end of Game.
        /// </summary>
        /// <param name="token"></param>
        /// <returns>True on success. If user isn't host, return false.</returns>
        internal async UniTask<Result> CloseCurrentLobby(CancellationToken token){
            Result canDestroy = await eosLobby.DestroyLobby(token);

            return canDestroy;
        }
    #region Offline Mode
        /// <summary>
        /// Just for solo mode like as tutorial. <br />
        /// This lobby is not connected to the network and backend and is just for a local Host. <br />
        /// When Quit OfflineMode, call MatchMakeManager.Instance.DestoryOfflineLobby or just Unity's Destory(MatchMakeManager.Instance)　and delete LobbyID method.<br />
        /// *No Timeout and failure
        /// </summary>
        /// <returns>Always return true. the LastResultCode becomes Success after return true.</returns>
        public async UniTask<Result> CreateOfflineLobby(Lobby lobbyCondition, OfflineMatchmakingDelay delay, List<AttributeData> userAttributes = null, CancellationTokenSource token = default(CancellationTokenSource)){
            bool needTryCatch = token == default;
            matchingToken = needTryCatch ? new CancellationTokenSource() : token;

            if(needTryCatch){
                try{
                    await eosLobby.CreateOfflineLobby(lobbyCondition, delay, userAttributes ?? new(), matchingToken.Token);
                }catch(OperationCanceledException){
                #if SYNICSUGAR_LOG
                    Debug.Log("MatchMaking is canceled");
                #endif
                    MatchMakingGUIEvents.ChangeState(MatchMakingGUIEvents.State.Standby);
                    return Result.Canceled;
                }
            }else{
                await eosLobby.CreateOfflineLobby(lobbyCondition, delay, userAttributes ?? new(), matchingToken.Token);
            }
            return Result.Success;
        }
        /// <summary>
        /// Just for solo mode like as tutorial. <br />
        /// Destory Lobby Instance. We can use just Destory(MatchMakeManager.Instance)　and delete LobbyID method without calling this.<br />
        /// </summary>
        /// <returns>Always return true. the LastResultCode becomes Success after return true.</returns>
        public async UniTask<Result> DestoryOfflineLobby(bool destroyManager = true){
            p2pConfig.Instance.connectionManager.p2pToken?.Cancel();
            await eosLobby.DestroyOfflineLobby();
            if(destroyManager){
                Destroy(this.gameObject);
            }
            return Result.Success;
        }
    #endregion
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
                    lobbyIDMethod.OnDelete();
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
        /// <summary>
        /// To check disconencted user's conenction state after p2p.
        /// </summary>
        /// <param name="disconenctedUserIndex"> UserIndex. For second Heart beat, +100</param>
        internal void UpdateMemberAttributeAsHeartBeat(int disconenctedUserIndex){
            eosLobby.UpdateMemberAttributeAsHeartBeat(disconenctedUserIndex);
        }
    }
}