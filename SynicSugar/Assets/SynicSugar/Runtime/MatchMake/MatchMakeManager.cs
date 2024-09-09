using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using SynicSugar.P2P;
using SynicSugar.Base;
using UnityEngine;
using UnityEngine.Events;
namespace SynicSugar.MatchMake {
    public class MatchMakeManager : MonoBehaviour {
#region Singleton
        public static MatchMakeManager Instance { get; private set; }
        void Awake() {
            if( Instance != null ) {
                Destroy( gameObject );
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            MemberUpdatedNotifier = new();

            if(lobbyIdSaveType == RecconectLobbyIdSaveType.CustomMethod){
                if(customSaveLobbyID != null && customDeleteLobbyID != null){
                    lobbyIDMethod.Save += () => customSaveLobbyID.Invoke();
                    lobbyIDMethod.Delete += () => customDeleteLobbyID.Invoke();
                }
            }
            isLooking = false;
            isConcluding = false;
            timeUntilTimeout = 0f;
        }
        void Start(){
            matchmakingCore = SynicSugarManger.Instance.CoreFactory.GetMatchmakingCore(maxSearchResult);
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
        /// When we need the more time than 10 minutes for timeout, we can set TimeoutSec directly. If the value is too small, matchmaking process will be canceled before waiting for opponents.<br />
        /// If call SetTimeoutSec after matchmaking has started could cause bugs, so set this in the Editor or call SetTimeoutSec before matchmaking.
        /// </summary>
        [Range(20, 600)]
        public ushort timeoutSec = 180;
        [Obsolete("Change will P2PSetupTimeoutSec to p2pSetupTimeoutSec.")]
        public ushort TimeoutSec { get { return TimeoutSec; } set { TimeoutSec = value;} }

        /// <summary>This time is from the start of preparation for p2p until the the end of the preparetion.<br />
        // If that time passes before matchmaking APIs return result, the matchmaking APIs return false as Timeout.<br />
        // When we need the more time than 1 minutes for timeout, we can set TimeoutSec directly.<br />
        /// If call SetTimeoutSec after matchmaking has started could cause bugs, so set this in the Editor or call SetTimeoutSec before matchmaking.
        /// </summary>
        [Range(5, 60)]
        public ushort p2pSetupTimeoutSec = 15;
        [Obsolete("Change will P2PSetupTimeoutSec to p2pSetupTimeoutSec.")]
        public ushort P2PSetupTimeoutSec { get { return p2pSetupTimeoutSec; } set { p2pSetupTimeoutSec = value;} }

        /// <summary>
        /// If true, pass host authority　to others when local user leave the lobby in timeout. <br />
        /// This flag is NOT related if user calls cancel apis.
        /// </summary>
        public bool enableHostmigrationInMatchmaking;


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
        internal MatchmakingCore matchmakingCore { get; private set; }

        /// <summary>
        /// This token manages the matching task, which is created internally when the API is called.  This cannot be touched from the outside.
        /// </summary>
        internal CancellationTokenSource matchmakeTokenSource;
        public MatchMakingGUIEvents MatchMakingGUIEvents = new MatchMakingGUIEvents();
        // Events
        public MemberUpdatedNotifier MemberUpdatedNotifier;

        /// <summary>
        /// This local user is waiting for opponents?
        /// </summary>
        public bool isLooking { get; private set; }
        /// <summary>
        /// This local user is waiting for opponents?
        /// </summary>
        public bool isConcluding { get; set; }

        /// <summary>
        /// Sec until stopping the process to wait for opponents.
        /// </summary>
        public float timeUntilTimeout { get; private set; }
        /// <summary>
        /// Is this user Host?
        /// </summary>
        public bool isHost { get { return matchmakingCore.isHost(); }}
        /// <summary>
        /// Is this id is LocalUser's id?
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool isLocalUserId(UserId id){
            return matchmakingCore.isLocalUser(id);
        }
        /// <summary>
        /// Is this id is LocalUser's id?
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool isLocalUserId(string id){
            return matchmakingCore.isLocalUser(UserId.GetUserId(id));
        }
        public int GetCurrentLobbyMemberCount(){
           return matchmakingCore.GetCurrentLobbyMemberCount();
        }

        public int GetMaxLobbyMemberCount(){
           return matchmakingCore.GetLobbyMemberLimit();
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
            timeoutSec = MatchmakingTimeout == 180 ? timeoutSec : MatchmakingTimeout;
            p2pSetupTimeoutSec = InitialConnectionTimeout == 15 ? p2pSetupTimeoutSec : InitialConnectionTimeout;
        }
    #region SearchAndCreateLobby
        /// <summary>
        /// MatchMake player with conditions and get the data for p2p connect. <br />
        /// Search a lobby, then if can't join, create a lobby as host. When lobby filled with max members, the lobby is closed automatically.
        /// </summary>
        /// <param name="lobbyCondition">Crate by EOSLobbyExtenstions.GenerateLobby().</param>
        /// <param name="token">For cancel matchmaking. If cancelled externally, all processes are cancelled via Timeout processing.</param>
        /// <returns></returns>
        public async UniTask<Result> SearchAndCreateLobby(Lobby lobbyCondition, CancellationToken token = default(CancellationToken)){
            if(SynicSugarManger.Instance.State.IsMatchmaking || SynicSugarManger.Instance.State.IsInSession){        
            #if SYNICSUGAR_LOG
                Debug.Log(SynicSugarManger.Instance.State.IsMatchmaking ? "SearchAndCreateLobby: Matchmaking is in progress" : "SearchAndCreateLobby: Currently in session.");
            #endif
                return Result.InvalidAPICall;
            }
            //Matchmaking
            Result matchmakingResult = await SearchAndCreateLobbyEntity(lobbyCondition, 0, new List<AttributeData>(), token);

            if(matchmakingResult != Result.Success){
                MatchMakingGUIEvents.ChangeState(MatchMakingGUIEvents.State.Standby);
                SynicSugarManger.Instance.State.IsMatchmaking = false;
                return matchmakingResult;
            }
            
            //p2p setup
            return await SetupP2P(false, token);
        }
        /// <summary>
        /// MatchMake player with conditions and get the data for p2p connect. <br />
        /// Search a lobby, then if can't join, create a lobby as host. After lobby filled with min members, Host closes lobby with FinishMatchmaking().
        /// </summary>
        /// <param name="lobbyCondition">Crate by EOSLobbyExtenstions.GenerateLobby().</param>
        /// <param name="minLobbyMember">Minimum number of lobby members required. <br />
        /// To close automatically, 0 or pass nothing. The case completes matchmaking on filled in lobby by max members. <br />
        /// If 2 or more, after lobby reach this min value,  (If we use EnableManualFinish event.) ManualFinish Button is displayed for Host. Host calls FinishMatchmake(), then the matchmaking is completed and start p2p. (If not call FinishMatchmake(), the matchmaking is going on until timeout and get failed.)</param>
        /// <param name="userAttributes">The user attributes of names, job and so on that is needed before P2P. <br />
        /// These should be used just for matchmaking and the kick, the data for actual game should be exchanged via p2p for the lag and server bandwidth .</param>
        /// <param name="token">For cancel matchmaking. If cancelled externally, all processes are cancelled via Timeout processing.</param>
        /// <returns></returns>
        public async UniTask<Result> SearchAndCreateLobby(Lobby lobbyCondition, uint minLobbyMember, List<AttributeData> userAttributes = null, CancellationToken token = default(CancellationToken)){    
            if(SynicSugarManger.Instance.State.IsMatchmaking || SynicSugarManger.Instance.State.IsInSession){        
            #if SYNICSUGAR_LOG
                Debug.Log(SynicSugarManger.Instance.State.IsMatchmaking ? "SearchAndCreateLobby: Matchmaking is in progress" : "SearchAndCreateLobby: Currently in session.");
            #endif
                return Result.InvalidAPICall;
            }
            //Matchmaking
            Result matchmakingResult = await SearchAndCreateLobbyEntity(lobbyCondition, minLobbyMember, userAttributes, token);

            if(matchmakingResult != Result.Success){
                MatchMakingGUIEvents.ChangeState(MatchMakingGUIEvents.State.Standby);
                SynicSugarManger.Instance.State.IsMatchmaking = false;
                return matchmakingResult;
            }
            
            //p2p setup
            return await SetupP2P(false, token);
        }
        async UniTask<Result> SearchAndCreateLobbyEntity(Lobby lobbyCondition, uint minLobbyMember, List<AttributeData> userAttributes, CancellationToken token){
            if(minLobbyMember < 2 || minLobbyMember > lobbyCondition.MaxLobbyMembers){
                minLobbyMember = 0;
            }
            matchmakeTokenSource = new CancellationTokenSource();
            
            TimeoutTimer(timeoutSec, token).Forget();

            try{
                Result result = await matchmakingCore.StartMatching(lobbyCondition, userAttributes, minLobbyMember, matchmakeTokenSource.Token);
                isLooking = false;
                return result;
            }catch(OperationCanceledException){
                MatchMakingGUIEvents.ChangeState(MatchMakingGUIEvents.State.Standby);
                isLooking = false;
                SynicSugarManger.Instance.State.IsMatchmaking = false;
                return Result.Canceled;
            }         
        }
    #endregion
    #region Just Search
        /// <summary>
        /// Search lobby to join, then get the data for p2p connect. <br />
        /// Recommend: SearchAndCreateLobby()
        /// </summary>
        /// <param name="lobbyCondition">Crate by EOSLobbyExtenstions.GenerateLobby().</param>
        /// <param name="token">For cancel matchmaking. If cancelled externally, all processes are cancelled via Timeout processing.</param>
        /// <returns></returns>
        public async UniTask<Result> SearchLobby(Lobby lobbyCondition, CancellationToken token = default(CancellationToken)){
            if(SynicSugarManger.Instance.State.IsMatchmaking || SynicSugarManger.Instance.State.IsInSession){        
            #if SYNICSUGAR_LOG
                Debug.Log(SynicSugarManger.Instance.State.IsMatchmaking ? "SearchLobby: Matchmaking is in progress" : "SearchLobby: Currently in session.");
            #endif
                return Result.InvalidAPICall;
            }
            //Matchmaking
            Result matchmakingResult = await SearchLobbyEntity(lobbyCondition, 0, new List<AttributeData>(), token);

            if(matchmakingResult != Result.Success){
                MatchMakingGUIEvents.ChangeState(MatchMakingGUIEvents.State.Standby);
                SynicSugarManger.Instance.State.IsMatchmaking = false;
                return matchmakingResult;
            }
            
            //p2p setup
            return await SetupP2P(false, token);
        }
        /// <summary>
        /// Search lobby to join, then get the data for p2p connect. <br />
        /// Recommend: SearchAndCreateLobby()
        /// </summary>
        /// <param name="lobbyCondition">Crate by EOSLobbyExtenstions.GenerateLobby().</param>
        /// <param name="minLobbyMember">Minimum number of lobby members required. <br />
        /// To close automatically, 0 or pass nothing. The case completes matchmaking on filled in lobby by max members. <br />
        /// If 2 or more, after lobby reach this min value,  (If we use EnableManualFinish event.) ManualFinish Button is displayed for Host. Host calls FinishMatchmake(), then the matchmaking is completed and start p2p. (If not call FinishMatchmake(), the matchmaking is going on until timeout and get failed.)</param>
        /// <param name="userAttributes">The user attributes of names, job and so on that is needed before P2P. <br />
        /// These should be used just for matchmaking and the kick, the data for actual game should be exchanged via p2p for the lag and server bandwidth .</param>
        /// <param name="token">For cancel matchmaking. If cancelled externally, all processes are cancelled via Timeout processing.</param>
        /// <returns></returns>
        public async UniTask<Result> SearchLobby(Lobby lobbyCondition, uint minLobbyMember, List<AttributeData> userAttributes = null, CancellationToken token = default(CancellationToken)){
            if(SynicSugarManger.Instance.State.IsMatchmaking || SynicSugarManger.Instance.State.IsInSession){        
            #if SYNICSUGAR_LOG
                Debug.Log(SynicSugarManger.Instance.State.IsMatchmaking ? "SearchLobby: Matchmaking is in progress" : "SearchLobby: Currently in session.");
            #endif
                return Result.InvalidAPICall;
            }
            //Matchmaking
            Result matchmakingResult = await SearchLobbyEntity(lobbyCondition, minLobbyMember, userAttributes, token);

            if(matchmakingResult != Result.Success){
                MatchMakingGUIEvents.ChangeState(MatchMakingGUIEvents.State.Standby);
                SynicSugarManger.Instance.State.IsMatchmaking = false;
                return matchmakingResult;
            }
            
            //p2p setup
            return await SetupP2P(false, token);
        }
        
        async UniTask<Result> SearchLobbyEntity(Lobby lobbyCondition, uint minLobbyMember, List<AttributeData> userAttributes, CancellationToken token){
            if(minLobbyMember < 2 || minLobbyMember > lobbyCondition.MaxLobbyMembers){
                minLobbyMember = 0;
            }
            matchmakeTokenSource = new CancellationTokenSource();
            
            TimeoutTimer(timeoutSec, token).Forget();

            try{
                Result result = await matchmakingCore.StartJustSearch(lobbyCondition, userAttributes, minLobbyMember, matchmakeTokenSource.Token);
                isLooking = false;
                return result;
            }catch(OperationCanceledException){
                MatchMakingGUIEvents.ChangeState(MatchMakingGUIEvents.State.Standby);
                isLooking = false;
                SynicSugarManger.Instance.State.IsMatchmaking = false;
                return Result.Canceled;
            }         
        }
    #endregion
    #region Just Create
        /// <summary>
        /// Create lobby and wait for other users, then get the data for p2p connect. <br />
        /// Recommend: SearchAndCreateLobby()
        /// </summary>
        /// <param name="lobbyCondition">Crate by EOSLobbyExtenstions.GenerateLobby().</param>
        /// <param name="token">For cancel matchmaking. If cancelled externally, all processes are cancelled via Timeout processing.</param>
        /// <returns></returns>
        public async UniTask<Result> CreateLobby(Lobby lobbyCondition, CancellationToken token = default(CancellationToken)){
            if(SynicSugarManger.Instance.State.IsMatchmaking || SynicSugarManger.Instance.State.IsInSession){        
            #if SYNICSUGAR_LOG
                Debug.Log(SynicSugarManger.Instance.State.IsMatchmaking ? "CreateLobby: Matchmaking is in progress" : "CreateLobby: Currently in session.");
            #endif
                return Result.InvalidAPICall;
            }
            //Matchmaking
            Result matchmakingResult = await SearchLobbyEntity(lobbyCondition, 0, new List<AttributeData>(), token);

            if(matchmakingResult != Result.Success){
                MatchMakingGUIEvents.ChangeState(MatchMakingGUIEvents.State.Standby);
                SynicSugarManger.Instance.State.IsMatchmaking = false;
                return matchmakingResult;
            }
            
            //p2p setup
            return await SetupP2P(false, token);
        }
        /// <summary>
        /// Create lobby and wait for other users, then get the data for p2p connect. <br />
        /// Recommend: SearchAndCreateLobby()
        /// </summary>
        /// <param name="lobbyCondition">Crate by EOSLobbyExtenstions.GenerateLobby().</param>
        /// <param name="minLobbyMember">Minimum number of lobby members required. <br />
        /// To close automatically, 0 or pass nothing. The case completes matchmaking on filled in lobby by max members. <br />
        /// If 2 or more, after lobby reach this min value,  (If we use EnableManualFinish event.) ManualFinish Button is displayed for Host. Host calls FinishMatchmake(), then the matchmaking is completed and start p2p. (If not call FinishMatchmake(), the matchmaking is going on until timeout and get failed.)</param>
        /// <param name="userAttributes">The user attributes of names, job and so on that is needed before P2P. <br />
        /// These should be used just for matchmaking and the kick, the data for actual game should be exchanged via p2p for the lag and server bandwidth .</param>
        /// <param name="token">For cancel matchmaking. If cancelled externally, all processes are cancelled via Timeout processing.</param>
        /// <returns></returns>
        public async UniTask<Result> CreateLobby(Lobby lobbyCondition, uint minLobbyMember, List<AttributeData> userAttributes = null, CancellationToken token = default(CancellationToken)){
            if(SynicSugarManger.Instance.State.IsMatchmaking || SynicSugarManger.Instance.State.IsInSession){        
            #if SYNICSUGAR_LOG
                Debug.Log(SynicSugarManger.Instance.State.IsMatchmaking ? "CreateLobby: Matchmaking is in progress" : "CreateLobby: Currently in session.");
            #endif
                return Result.InvalidAPICall;
            }
            
            Result matchmakingResult = await CreateLobbyEntity(lobbyCondition, minLobbyMember, userAttributes, token);

            if(matchmakingResult != Result.Success){
                MatchMakingGUIEvents.ChangeState(MatchMakingGUIEvents.State.Standby);
                SynicSugarManger.Instance.State.IsMatchmaking = false;
                return matchmakingResult;
            }
            
            //p2p setup
            return await SetupP2P(false, token);
        }

        async UniTask<Result> CreateLobbyEntity(Lobby lobbyCondition, uint minLobbyMember, List<AttributeData> userAttributes, CancellationToken token){
            if(minLobbyMember < 2 || minLobbyMember > lobbyCondition.MaxLobbyMembers){
                minLobbyMember = 0;
            }
            matchmakeTokenSource = new CancellationTokenSource();
            
            TimeoutTimer(timeoutSec, token).Forget();

            try{
                Result result = await matchmakingCore.StartJustCreate(lobbyCondition, userAttributes, minLobbyMember, token);
                isLooking = false;
                return result;
            }catch(OperationCanceledException){
                MatchMakingGUIEvents.ChangeState(MatchMakingGUIEvents.State.Standby);
                isLooking = false;
                SynicSugarManger.Instance.State.IsMatchmaking = false;
                return Result.Canceled;
            }         
        }
    #endregion
        /// <summary>
        /// Join the Lobby with saved LobbyID. <br />
        /// Call this at the start of game or match-make.
        /// </summary>
        /// <param name="LobbyID">Lobby ID to <c>re</c>-connect</param>
        /// <param name="token">For cancel matchmaking. If cancelled externally, all processes are cancelled via Timeout processing.</param>
        public async UniTask<Result> ReconnectLobby(string LobbyID, CancellationToken token = default(CancellationToken)){
            if(SynicSugarManger.Instance.State.IsMatchmaking || SynicSugarManger.Instance.State.IsInSession){        
            #if SYNICSUGAR_LOG
                Debug.Log(SynicSugarManger.Instance.State.IsMatchmaking ? "ReconnectLobby: Matchmaking is in progress" : "ReconnectLobby: Currently in session.");
            #endif
                return Result.InvalidAPICall;
            }
            if(string.IsNullOrEmpty(LobbyID)){
             #if SYNICSUGAR_LOG
                Debug.Log("ReconnectLobby: LobbyId is empty.");
            #endif
                return Result.InvalidParameters;
            }
    #if SYNICSUGAR_LOG
            Debug.Log($"Try Recconect with {LobbyID}");
    #endif
            isLooking = true;
            SynicSugarManger.Instance.State.IsMatchmaking = true;
            try{
                Result joinResult = await matchmakingCore.JoinLobbyBySavedLobbyId(LobbyID, token);

                isLooking = false;
                if(joinResult != Result.Success){
                    MatchMakingGUIEvents.ChangeState(MatchMakingGUIEvents.State.Standby);
                    SynicSugarManger.Instance.State.IsMatchmaking = false;
                    return joinResult;
                }
            }catch(OperationCanceledException){
                MatchMakingGUIEvents.ChangeState(MatchMakingGUIEvents.State.Standby);
                isLooking = false;
                SynicSugarManger.Instance.State.IsMatchmaking = false;
                return Result.Canceled;
            }         

            return await SetupP2P(true, token);
        }

        /// <summary>
        /// Wait for timeout. When pass time, stop matchmake task.
        /// </summary>
        /// <param name="timeoutSec"></param>
        /// <param name="userToken">Token for the function to be passed from API caller.</param>
        /// <returns></returns>
        async UniTask TimeoutTimer(int timeoutSec, CancellationToken userToken){
            isLooking = true;
            SynicSugarManger.Instance.State.IsMatchmaking = true;
            timeUntilTimeout = timeoutSec;
            try{
                while(timeUntilTimeout > 0f && isLooking){
                    timeUntilTimeout -= Time.deltaTime;
                    await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken: userToken);
                }
            }
            catch (OperationCanceledException){ // Cancel matchmaking process by user from library outside.
            #if SYNICSUGAR_LOG
                if(timeUntilTimeout > 0){
                    Debug.Log("Canceld matching by cancel token from MatchMakeManager outside.");
                }else{
                    Debug.Log("Cancel matching by timeout");
                }
            #endif
                matchmakeTokenSource?.Cancel();
                return;
            }
        #if SYNICSUGAR_LOG
            Debug.Log("MatchMakeManager: Stop timeout timer for looking opponents.");
        #endif
            //Cancel matchmaking in timeout(=isLooking)
            //or the matchmaking is complete(=!isLooking).
            if(isLooking){
            #if SYNICSUGAR_LOG
                Debug.Log("Cancel matching by timeout");
            #endif
                matchmakeTokenSource?.Cancel();
                isLooking = false;
            }
        }
        /// <summary>
        /// Call this after matchmaking to prepare for p2p connection.
        /// </summary>
        /// <param name="isReconencter">If true, create UserIds class as Reconnecter.</param>
        /// <returns></returns>
        async UniTask<Result> SetupP2P(bool isReconencter, CancellationToken token){
            p2pInfo.Instance.userIds = new UserIds(isReconencter);

            Result setupResult = await matchmakingCore.SetupP2PConnection(p2pSetupTimeoutSec, token);

            if(setupResult != Result.Success){
                MatchMakingGUIEvents.ChangeState(MatchMakingGUIEvents.State.Standby);
                return setupResult;
            }

            MatchMakingGUIEvents.ChangeState(MatchMakingGUIEvents.State.Ready);

            SynicSugarManger.Instance.State.IsMatchmaking = false;
            SynicSugarManger.Instance.State.IsInSession = true;
            return Result.Success;
        }

        /// <summary>
        /// After the requirement is met, Host manually finish matchmaking and start p2p.
        /// </summary>
        public void ConcludeMatchMake(){
            if(isConcluding || !isHost){
                Debug.LogError("ConcludeMatchMake: This user can't conclude lobby now.");
                return;
            }
            isConcluding = true;
            matchmakingCore.SwitchLobbyAttribute();
            isConcluding = false;
        }
        
        /// <summary>
        /// Exit lobby and cancel MatchMake. <br />
        /// When Host calls this method, Guest will becomes new Host automatically after Host-migration. <br />
        /// When this process is called, Timer process that was calculating the Timeout stops, and the API that exits or closes the Lobby is called.
        /// Then, after this process returns Result.Success result, the MatchmakingAPIs starts the process to finish the Matchmaking Task and returns Result.LobbyClosed on success.
        /// This process is valid only during matchmaking.　To finish Connection after matchmaking, we use ConnectHub.Instance.ExitSession and ConnectHub.Instance.CloseSession.
        /// </summary>
        /// <param name="destroyManager">If true, destroy NetworkManager after cancel matchmake.</param>
        /// <param name="token">token for this task</param>
        /// <returns></returns>
        public async UniTask<Result> ExitCurrentMatchMake(bool destroyManager = true, CancellationToken token = default(CancellationToken)){
            if(!SynicSugarManger.Instance.State.IsMatchmaking || !isLooking){
            #if SYNICSUGAR_LOG
                Debug.Log("ExitCurrentMatchMake: This user is not in matchmaking now.");
            #endif
                return Result.InvalidAPICall;
            }
            isLooking = false;
            Result cancelResult = await matchmakingCore.CancelMatchMaking(token);
            
            if(cancelResult != Result.Success){
                return cancelResult;
            }
            if(destroyManager){
                Destroy(gameObject);
            }
            SynicSugarManger.Instance.State.IsMatchmaking = false;

            return Result.Success;
        }
        /// <summary>
        /// If Host, destroy lobby and cancels MatchMake.<br />
        /// If Guest, just leave lobby and cancels MatchMake.<br />
        /// When this process is called, Timer process that was calculating the Timeout stops, and the API that exits or closes the Lobby is called.
        /// Then, after this process returns Result.Success result, the MatchmakingAPIs starts the process to finish the Matchmaking Task and returns Result.LobbyClosed on success. <br />
        /// This process is valid only during matchmaking.　To finish Connection after matchmaking, we use ConnectHub.Instance.ExitSession and ConnectHub.Instance.CloseSession.
        /// </summary>
        /// <param name="destroyManager">If true, destroy NetworkManager after cancel matchmake.</param>
        /// <param name="token">token for this task</param>
        /// <returns></returns>
        public async UniTask<Result> CloseCurrentMatchMake(bool destroyManager = true, CancellationToken token = default(CancellationToken)){
            if(!SynicSugarManger.Instance.State.IsMatchmaking || !isLooking){
            #if SYNICSUGAR_LOG
                Debug.Log("CloseCurrentMatchMake: This user is not in matchmaking now.");
            #endif
                return Result.InvalidAPICall;
            }
            isLooking = false;
            Result closeResult = await matchmakingCore.CloseMatchMaking(token);
            
            if(closeResult != Result.Success){
                return closeResult;
            }
            if(destroyManager){
                Destroy(gameObject);
            }
            SynicSugarManger.Instance.State.IsMatchmaking = false;

            return Result.Success;
        }
        
        /// <summary>
        /// Host kicks a specific target from Lobby. Only one tareget can be kicked at a time.
        /// </summary>
        /// <param name="token">token for this task</param>
        /// <returns></returns>
        public async UniTask<Result> KickTargetFromLobby(UserId targetId, CancellationToken token = default(CancellationToken)){
            Result result = await matchmakingCore.KickTargetMember(targetId, token);

            return result;
        }
        /// <summary>
        /// Leave the current lobby in Game.
        /// </summary>
        /// <param name="cleanupMemberCountChanged">Need to call MatchMakeManager.Instance.MatchMakingGUIEvents.LobbyMemberCountChanged(id, false) after exit lobby?</param>
        /// <param name="token">Token for this task</param>
        internal async UniTask<Result> ExitCurrentLobby(bool cleanupMemberCountChanged, CancellationToken token){
            Result canDestroy = await matchmakingCore.LeaveLobby(token);

            if(canDestroy == Result.Success && cleanupMemberCountChanged){
                //To delete member object.
                foreach(var id in p2pInfo.Instance.AllUserIds){
                    MatchMakingGUIEvents.LobbyMemberCountChanged(id, false);
                }
            }

            return canDestroy;
        }
        /// <summary>
        /// Destroy the current lobby on the end of Game.
        /// </summary>
        /// <param name="cleanupMemberCountChanged">Need to call MatchMakeManager.Instance.MatchMakingGUIEvents.LobbyMemberCountChanged(id, false) after exit lobby?</param>
        /// <param name="token">Token for this task</param>
        /// <returns>True on success. If user isn't host, return false.</returns>
        internal async UniTask<Result> CloseCurrentLobby(bool cleanupMemberCountChanged, CancellationToken token){
            Result canDestroy = await matchmakingCore.DestroyLobby(token);

            if(canDestroy == Result.Success && cleanupMemberCountChanged){
                //To delete member object.
                foreach(var id in p2pInfo.Instance.AllUserIds){
                    MatchMakingGUIEvents.LobbyMemberCountChanged(id, false);
                }
            }
            return canDestroy;
        }
    #region Offline Mode
        /// <summary>
        /// Just for solo mode like as tutorial. <br />
        /// This lobby is not connected to the network and backend and is just for a local Host. <br />
        /// When Quit OfflineMode, call MatchMakeManager.Instance.DestoryOfflineLobby or just Unity's Destory(MatchMakeManager.Instance)　and delete LobbyID method.<br />
        /// *No Timeout and failure
        /// </summary>
        /// <param name="lobbyCondition">Crate by EOSLobbyExtenstions.GenerateLobby().</param>
        /// <param name="delay">Delay to simulate matchmaking. If pass NoDelay for this, Matchmaking GUI Events are NOT invoked on each step.<br />
        /// To close automatically, 0 or pass nothing. The case completes matchmaking on filled in lobby by max members. <br />
        /// If 2 or more, after lobby reach this min value,  (If we use EnableManualFinish event.) ManualFinish Button is displayed for Host. Host calls FinishMatchmake(), then the matchmaking is completed and start p2p. (If not call FinishMatchmake(), the matchmaking is going on until timeout and get failed.)</param>
        /// <param name="userAttributes">The user attributes of names, job and so on that is needed before P2P. <br />
        /// These should be used just for matchmaking and the kick, the data for actual game should be exchanged via p2p for the lag and server bandwidth .</param>
        /// <param name="token">For cancel matchmaking. If cancelled externally, all processes are cancelled via Timeout processing.</param>
        /// <returns>Always return true. the LastResultCode becomes Success after return true.</returns>
        public async UniTask<Result> CreateOfflineLobby(Lobby lobbyCondition, OfflineMatchmakingDelay delay, List<AttributeData> userAttributes = null, CancellationToken token = default(CancellationToken)){
            if(SynicSugarManger.Instance.State.IsMatchmaking || SynicSugarManger.Instance.State.IsInSession){        
            #if SYNICSUGAR_LOG
                Debug.Log(SynicSugarManger.Instance.State.IsMatchmaking ? "CreateOfflineLobby: Matchmaking is in progress" : "CreateOfflineLobby: Currently in session.");
            #endif
                return Result.InvalidAPICall;
            }
            SynicSugarManger.Instance.State.IsMatchmaking = true;
            
            try{
                isLooking = true;
                Result result = await matchmakingCore.CreateOfflineLobby(lobbyCondition, delay, userAttributes ?? new(), token);
                isLooking = false;
                SynicSugarManger.Instance.State.IsMatchmaking = false;
                SynicSugarManger.Instance.State.IsInSession = true;
                return result;
            }catch(OperationCanceledException){
            #if SYNICSUGAR_LOG
                Debug.Log("MatchMaking is canceled");
            #endif 
                isLooking = false;
                SynicSugarManger.Instance.State.IsMatchmaking = false;
                MatchMakingGUIEvents.ChangeState(MatchMakingGUIEvents.State.Standby);
                return Result.Canceled;
            }
        }
        /// <summary>
        /// Just for solo mode like as tutorial. <br />
        /// Destory Lobby Instance. We can use just Destory(MatchMakeManager.Instance)　and delete LobbyID method without calling this.<br />
        /// </summary>
        /// <param name="cleanupMemberCountChanged">If true, destroy NetworkManager after cancel matchmake.</param>
        /// <returns>Always return true. the LastResultCode becomes Success after return true.</returns>
        internal async UniTask<Result> DestoryOfflineLobby(bool cleanupMemberCountChanged, CancellationToken token){
            await matchmakingCore.DestroyOfflineLobby(token);

            if(cleanupMemberCountChanged){
                MatchMakingGUIEvents.LobbyMemberCountChanged(p2pInfo.Instance.LocalUserId, false);
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
            return matchmakingCore.GetCurrentLobbyID();
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
            return PlayerPrefs.GetString (playerprefsSaveKey, string.Empty);
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
            return matchmakingCore.GetTargetAttributeData(target, Key);
        }
        /// <summary>
        /// Get target all attributes.
        /// </summary>
        /// <param name="target">target user id</param>
        /// <returns>If no data, return null.</returns>
        public List<AttributeData> GetTargetAttributeData(UserId target){
            return matchmakingCore.GetTargetAttributeData(target);
        }

        /// <summary>
        /// To check disconencted user's conenction state after p2p.
        /// </summary>
        /// <param name="disconenctedUserIndex"> UserIndex. For second Heart beat, +100</param>
        internal void HeartBeatToLobby(int disconenctedUserIndex){
            matchmakingCore.UpdateMemberAttributeAsHeartBeat(disconenctedUserIndex);
        }

        #region OBSOLATE
        /// <summary>
        /// MatchMake player with conditions and get the data for p2p connect. <br />
        /// Search a lobby, then if can't join, create a lobby as host. When lobby filled with max members, the lobby is closed automatically.
        /// </summary>
        /// <param name="lobbyCondition">Crate by EOSLobbyExtenstions.GenerateLobby().</param>
        /// <param name="token">For cancel matchmaking. If cancelled externally, all processes are cancelled via Timeout processing.</param>
        /// <returns></returns>
        [Obsolete("This is old. Can use new one that pass the CancellationToken instead of CancellationTokenSource.")]
        public async UniTask<Result> SearchAndCreateLobby(Lobby lobbyCondition, CancellationTokenSource token){
            if(isLooking){
                Debug.Log("Matchmaking is in progress");
                return Result.InvalidAPICall;
            }
            Result matchmakingResult = await SearchAndCreateLobbyEntity(lobbyCondition, 0, new List<AttributeData>(), matchmakeTokenSource.Token);

            if(matchmakingResult != Result.Success){
                MatchMakingGUIEvents.ChangeState(MatchMakingGUIEvents.State.Standby);
                return matchmakingResult;
            }
            
            //p2p setup
            return await SetupP2P(false, token.Token);
        }
        /// <summary>
        /// MatchMake player with conditions and get the data for p2p connect. <br />
        /// Search a lobby, then if can't join, create a lobby as host. After lobby filled with min members, Host closes lobby with FinishMatchmaking().
        /// </summary>
        /// <param name="lobbyCondition">Crate by EOSLobbyExtenstions.GenerateLobby().</param>
        /// <param name="minLobbyMember">Minimum number of lobby members required. <br />
        /// To close automatically, 0 or pass nothing. The case completes matchmaking on filled in lobby by max members. <br />
        /// If 2 or more, after lobby reach this min value,  (If we use EnableManualFinish event.) ManualFinish Button is displayed for Host. Host calls FinishMatchmake(), then the matchmaking is completed and start p2p. (If not call FinishMatchmake(), the matchmaking is going on until timeout and get failed.)</param>
        /// <param name="userAttributes">The user attributes of names, job and so on that is needed before P2P. <br />
        /// These should be used just for matchmaking and the kick, the data for actual game should be exchanged via p2p for the lag and server bandwidth .</param>
        /// <param name="token">For cancel matchmaking. If cancelled externally, all processes are cancelled via Timeout processing.</param>
        /// <returns></returns>
        [Obsolete("This is old. Can use new one that pass the CancellationToken instead of CancellationTokenSource.")]
        public async UniTask<Result> SearchAndCreateLobby(Lobby lobbyCondition, uint minLobbyMember, List<AttributeData> userAttributes, CancellationTokenSource token){    
            if(isLooking){
                Debug.Log("Matchmaking is in progress");
                return Result.InvalidAPICall;
            }
            Result matchmakingResult = await SearchAndCreateLobbyEntity(lobbyCondition, minLobbyMember, userAttributes, matchmakeTokenSource.Token);

            if(matchmakingResult != Result.Success){
                MatchMakingGUIEvents.ChangeState(MatchMakingGUIEvents.State.Standby);
                return matchmakingResult;
            }
            
            //p2p setup
            return await SetupP2P(false, token.Token);
        }

        /// <summary>
        /// Search lobby to join, then get the data for p2p connect. <br />
        /// Recommend: SearchAndCreateLobby()
        /// </summary>
        /// <param name="lobbyCondition">Crate by EOSLobbyExtenstions.GenerateLobby().</param>
        /// <param name="token">For cancel matchmaking. If cancelled externally, all processes are cancelled via Timeout processing.</param>
        /// <returns></returns>
        [Obsolete("This is old. Can use new one that pass the CancellationToken instead of CancellationTokenSource.")]
        public async UniTask<Result> SearchLobby(Lobby lobbyCondition, CancellationTokenSource token){
            if(isLooking){
                Debug.Log("Matchmaking is in progress");
                return Result.InvalidAPICall;
            }
            
            Result matchmakingResult = await SearchLobbyEntity(lobbyCondition, 0, new List<AttributeData>(), matchmakeTokenSource.Token);

            if(matchmakingResult != Result.Success){
                MatchMakingGUIEvents.ChangeState(MatchMakingGUIEvents.State.Standby);
                return matchmakingResult;
            }
            
            //p2p setup
            return await SetupP2P(false, token.Token);
        }

        /// <summary>
        /// Search lobby to join, then get the data for p2p connect. <br />
        /// Recommend: SearchAndCreateLobby()
        /// </summary>
        /// <param name="lobbyCondition">Crate by EOSLobbyExtenstions.GenerateLobby().</param>
        /// <param name="minLobbyMember">Minimum number of lobby members required. <br />
        /// To close automatically, 0 or pass nothing. The case completes matchmaking on filled in lobby by max members. <br />
        /// If 2 or more, after lobby reach this min value,  (If we use EnableManualFinish event.) ManualFinish Button is displayed for Host. Host calls FinishMatchmake(), then the matchmaking is completed and start p2p. (If not call FinishMatchmake(), the matchmaking is going on until timeout and get failed.)</param>
        /// <param name="userAttributes">The user attributes of names, job and so on that is needed before P2P. <br />
        /// These should be used just for matchmaking and the kick, the data for actual game should be exchanged via p2p for the lag and server bandwidth .</param>
        /// <param name="token">For cancel matchmaking. If cancelled externally, all processes are cancelled via Timeout processing.</param>
        /// <returns></returns>
        [Obsolete("This is old. Can use new one that pass the CancellationToken instead of CancellationTokenSource.")]
        public async UniTask<Result> SearchLobby(Lobby lobbyCondition, uint minLobbyMember, List<AttributeData> userAttributes, CancellationTokenSource token){
            if(isLooking){
                Debug.Log("Matchmaking is in progress");
                return Result.InvalidAPICall;
            }
            
            Result matchmakingResult = await SearchLobbyEntity(lobbyCondition, minLobbyMember, userAttributes, matchmakeTokenSource.Token);

            if(matchmakingResult != Result.Success){
                MatchMakingGUIEvents.ChangeState(MatchMakingGUIEvents.State.Standby);
                return matchmakingResult;
            }
            
            //p2p setup
            return await SetupP2P(false, token.Token);
        }

        /// <summary>
        /// Create lobby and wait for other users, then get the data for p2p connect. <br />
        /// Recommend: SearchAndCreateLobby()
        /// </summary>
        /// <param name="lobbyCondition">Crate by EOSLobbyExtenstions.GenerateLobby().</param>
        /// <param name="token">For cancel matchmaking. If cancelled externally, all processes are cancelled via Timeout processing.</param>
        /// <returns></returns>
        [Obsolete("This is old. Can use new one that pass the CancellationToken instead of CancellationTokenSource.")]
        public async UniTask<Result> CreateLobby(Lobby lobbyCondition, CancellationTokenSource token = default(CancellationTokenSource)){
            if(isLooking){
                Debug.Log("Matchmaking is in progress");
                return Result.InvalidAPICall;
            }
            
            Result matchmakingResult = await SearchLobbyEntity(lobbyCondition, 0, new List<AttributeData>(), matchmakeTokenSource.Token);

            if(matchmakingResult != Result.Success){
                MatchMakingGUIEvents.ChangeState(MatchMakingGUIEvents.State.Standby);
                return matchmakingResult;
            }
            
            //p2p setup
            return await SetupP2P(false, token.Token);
        }
        /// <summary>
        /// Create lobby and wait for other users, then get the data for p2p connect. <br />
        /// Recommend: SearchAndCreateLobby()
        /// </summary>
        /// <param name="lobbyCondition">Crate by EOSLobbyExtenstions.GenerateLobby().</param>
        /// <param name="minLobbyMember">Minimum number of lobby members required. <br />
        /// To close automatically, 0 or pass nothing. The case completes matchmaking on filled in lobby by max members. <br />
        /// If 2 or more, after lobby reach this min value,  (If we use EnableManualFinish event.) ManualFinish Button is displayed for Host. Host calls FinishMatchmake(), then the matchmaking is completed and start p2p. (If not call FinishMatchmake(), the matchmaking is going on until timeout and get failed.)</param>
        /// <param name="userAttributes">The user attributes of names, job and so on that is needed before P2P. <br />
        /// These should be used just for matchmaking and the kick, the data for actual game should be exchanged via p2p for the lag and server bandwidth .</param>
        /// <param name="token">For cancel matchmaking. If cancelled externally, all processes are cancelled via Timeout processing.</param>
        /// <returns></returns>
        [Obsolete("This is old. Can use new one that pass the CancellationToken instead of CancellationTokenSource.")]
        public async UniTask<Result> CreateLobby(Lobby lobbyCondition, uint minLobbyMember, List<AttributeData> userAttributes, CancellationTokenSource token){
            if(isLooking){
                Debug.Log("Matchmaking is in progress");
                return Result.InvalidAPICall;
            }
            
            Result matchmakingResult = await CreateLobbyEntity(lobbyCondition, minLobbyMember, userAttributes, matchmakeTokenSource.Token);

            if(matchmakingResult != Result.Success){
                MatchMakingGUIEvents.ChangeState(MatchMakingGUIEvents.State.Standby);
                return matchmakingResult;
            }
            
            //p2p setup
            return await SetupP2P(false, token.Token);
        }

        /// <summary>
        /// Join the Lobby with saved LobbyID. <br />
        /// Call this at the start of game or match-make.
        /// </summary>
        /// <param name="LobbyID">Lobby ID to <c>re</c>-connect</param>
        /// <param name="token">For cancel matchmaking. If cancelled externally, all processes are cancelled via Timeout processing.</param>
        [Obsolete("This is old. Can use new one that pass the CancellationToken instead of CancellationTokenSource.")]
        public async UniTask<Result> ReconnectLobby(string LobbyID, CancellationTokenSource token){
            if(string.IsNullOrEmpty(LobbyID)){
                return Result.InvalidParameters;
            }
    #if SYNICSUGAR_LOG
            Debug.Log($"Try Recconect with {LobbyID}");
    #endif
            matchmakeTokenSource = new CancellationTokenSource();
            
            try{
                Result joinResult = await matchmakingCore.JoinLobbyBySavedLobbyId(LobbyID, token.Token);

                if(joinResult != Result.Success){
                    MatchMakingGUIEvents.ChangeState(MatchMakingGUIEvents.State.Standby);
                    return joinResult;
                }
            }catch(OperationCanceledException){
                MatchMakingGUIEvents.ChangeState(MatchMakingGUIEvents.State.Standby);
                return Result.Canceled;
            }         

            return await SetupP2P(true, token.Token);
        }


        /// <summary>
        /// Just for solo mode like as tutorial. <br />
        /// This lobby is not connected to the network and backend and is just for a local Host. <br />
        /// When Quit OfflineMode, call MatchMakeManager.Instance.DestoryOfflineLobby or just Unity's Destory(MatchMakeManager.Instance)　and delete LobbyID method.<br />
        /// *No Timeout and failure
        /// </summary>
        /// <returns>Always return true. the LastResultCode becomes Success after return true.</returns>
        [Obsolete("This is old. Can use new one that pass the CancellationToken instead of CancellationTokenSource.")]
        public async UniTask<Result> CreateOfflineLobby(Lobby lobbyCondition, OfflineMatchmakingDelay delay, List<AttributeData> userAttributes, CancellationTokenSource token){
            try{
                await matchmakingCore.CreateOfflineLobby(lobbyCondition, delay, userAttributes ?? new(), token.Token);
            }catch(OperationCanceledException){
            #if SYNICSUGAR_LOG
                Debug.Log("MatchMaking is canceled");
            #endif
                MatchMakingGUIEvents.ChangeState(MatchMakingGUIEvents.State.Standby);
                return Result.Canceled;
            }

            return Result.Success;
        }

        /// <summary>
        /// Just for solo mode like as tutorial. <br />
        /// Destory Lobby Instance. We can use just Destory(MatchMakeManager.Instance)　and delete LobbyID method without calling this.<br />
        /// </summary>
        /// <param name="destroyManager">If true, destroy NetworkManager after cancel matchmake.</param>
        /// <returns>Always return true. the LastResultCode becomes Success after return true.</returns>
        [Obsolete("This is old. ConnectHub.Instance.DestoryOfflineLobby is new one")]
        public async UniTask<Result> DestoryOfflineLobby(bool destroyManager = true){
            if(!SynicSugarManger.Instance.State.IsInSession || p2pInfo.Instance.userIds.AllUserIds.Count != 1){
            #if SYNICSUGAR_LOG
                Debug.Log("DestoryOfflineLobby: This user dosen't have OfflineLobby.");
            #endif
                return Result.InvalidAPICall;
            }
            p2pConfig.Instance.sessionCore.CancelRTTToken();
            await matchmakingCore.DestroyOfflineLobby(this.GetCancellationTokenOnDestroy());
            
            Destroy(gameObject);

            SynicSugarManger.Instance.State.IsInSession = false;
            return Result.Success;
        }
        #endregion
    }
}