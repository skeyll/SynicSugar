using System;
using System.Threading;
using Cysharp.Threading.Tasks;
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

            if(lobbyIdSaveType == RecconectLobbyIdSaveType.CustomMethod){
                if(customSaveLobbyID != null && customDeleteLobbyID != null){
                    lobbyIDMethod.Save += () => customSaveLobbyID.Invoke();
                    lobbyIDMethod.Delete += () => customDeleteLobbyID.Invoke();
                }
            }
        }
        void OnDestroy() {
            if( Instance == this ) {
                //For sub events
                lobbyIDMethod.Clear();
                asyncLobbyIDMethod.Clear();

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
        EOSLobby eosLobby;
        internal CancellationTokenSource matchingToken;
        public MatchGUIState matchState = new MatchGUIState();

        public int GetCurrentLobbyMemberCount(){
           return eosLobby.GetCurrentLobbyMemberCount();
        }

        /// <summary>
        /// Set State from script
        /// </summary>
        /// <param name="state"></param>
        public void SetGUIState(MatchGUIState state){
            matchState = state;
        }

        /// <summary>
        /// MatchMake player with conditions and get the data for p2p connect. <br />
        /// Search a lobby, then if can't join, create a lobby as host.
        /// </summary>
        /// <param name="lobbyCondition">Crate by EOSLobbyExtenstions.GenerateLobby().</param>
        /// <param name="token">For cancel matchmaking. This is used by CancelCurrentMatchMake.
        /// If pass, we implement OperationCanceledException by ourself.
        /// If not pass, such processe are done internally and return false when we cancel matchmake.</param>
        /// <returns></returns>
        public async UniTask<bool> SearchAndCreateLobby(Lobby lobbyCondition, CancellationTokenSource token = default(CancellationTokenSource)){
            bool useTryCatch = token == default;
            matchingToken = useTryCatch ? new CancellationTokenSource() : token;
            
            bool canMatch = false;

            //Match at Lobby
            if(useTryCatch){
                try{
                canMatch = await eosLobby.StartMatching(lobbyCondition, matchingToken.Token);
                }catch(OperationCanceledException){
                #if SYNICSUGAR_LOG
                    Debug.Log("MatchMaking is canceled");
                #endif
                    UpdateStateDescription(MatchState.Cancel);
                    return false;
                }
            }else{
                canMatch = await eosLobby.StartMatching(lobbyCondition, matchingToken.Token);
            }

            if(!canMatch){
                UpdateStateDescription(MatchState.Cancel);
                return false;
            }
            
            UpdateStateDescription(MatchState.Success);
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
        /// <returns></returns>
        public async UniTask<bool> SearchLobby(Lobby lobbyCondition, CancellationTokenSource token = default(CancellationTokenSource)){
            bool useTryCatch = token == default;
            matchingToken = useTryCatch ? new CancellationTokenSource() : token;
            
            bool canMatch = false;

            //Match at Lobby
            if(useTryCatch){
                try{
                canMatch = await eosLobby.StartJustSearch(lobbyCondition, matchingToken.Token);
                }catch(OperationCanceledException){
                #if SYNICSUGAR_LOG
                    Debug.Log("MatchMaking is canceled");
                #endif
                    UpdateStateDescription(MatchState.Cancel);
                    return false;
                }
            }else{
                canMatch = await eosLobby.StartJustSearch(lobbyCondition, matchingToken.Token);
            }


            if(!canMatch){
                UpdateStateDescription(MatchState.Cancel);
                return false;
            }
            
            UpdateStateDescription(MatchState.Success);
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
        /// <returns></returns>
        public async UniTask<bool> CreateLobby(Lobby lobbyCondition, CancellationTokenSource token = default(CancellationTokenSource)){
            bool useTryCatch = token == default;
            matchingToken = useTryCatch ? new CancellationTokenSource() : token;
            
            bool canMatch = false;

            //Match at Lobby
            if(useTryCatch){
                try{
                canMatch = await eosLobby.StartJustCreate(lobbyCondition, matchingToken.Token);
                }catch(OperationCanceledException){
                #if SYNICSUGAR_LOG
                    Debug.Log("MatchMaking is canceled");
                #endif
                    UpdateStateDescription(MatchState.Cancel);
                    return false;
                }
            }else{
                canMatch = await eosLobby.StartJustCreate(lobbyCondition, matchingToken.Token);
            }

            if(!canMatch){
                UpdateStateDescription(MatchState.Cancel);
                return false;
            }
            
            UpdateStateDescription(MatchState.Success);
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
    
            bool useTryCatch = token == default;
            matchingToken = useTryCatch ? new CancellationTokenSource() : token;
            
            bool canJoin = false;

            //Match at Lobby
            if(useTryCatch){
                try{
                canJoin = await eosLobby.JoinLobbyBySavedLobbyId(LobbyID, matchingToken.Token);
                }catch(OperationCanceledException){
                #if SYNICSUGAR_LOG
                    Debug.Log("MatchMaking is canceled");
                #endif
                    UpdateStateDescription(MatchState.Cancel);
                    return false;
                }
            }else{
                canJoin = await eosLobby.JoinLobbyBySavedLobbyId(LobbyID, matchingToken.Token);
            }

            if(!canJoin){
                UpdateStateDescription(MatchState.Cancel);
                return false;
            }

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
            bool canCancel =  await eosLobby.CancelMatchMaking(matchingToken, token);
            
            if(removeManager && canCancel){
                Destroy(this.gameObject);
            }
            return canCancel;
        }
        /// <summary>
        /// Leave the current lobby in Game.
        /// </summary>
        /// <param name="token"></param>
        internal async UniTask<bool> ExitCurrentLobby(CancellationToken token){
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
                case MatchMakeManager.RecconectLobbyIdSaveType.NoReconnection:
                return;
                case MatchMakeManager.RecconectLobbyIdSaveType.Playerprefs:
                    PlayerPrefs.SetString(MatchMakeManager.Instance.playerprefsSaveKey, GetCurrentLobbyID());
                return;
                case MatchMakeManager.RecconectLobbyIdSaveType.CustomMethod:
                    lobbyIDMethod.OnSave();
                return;
                case MatchMakeManager.RecconectLobbyIdSaveType.AsyncCustomMethod:
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
                case MatchMakeManager.RecconectLobbyIdSaveType.NoReconnection:
                return;
                case MatchMakeManager.RecconectLobbyIdSaveType.Playerprefs:
                    PlayerPrefs.DeleteKey(MatchMakeManager.Instance.playerprefsSaveKey);
                return;
                case MatchMakeManager.RecconectLobbyIdSaveType.CustomMethod:
                    lobbyIDMethod.OnSave();
                return;
                case MatchMakeManager.RecconectLobbyIdSaveType.AsyncCustomMethod:
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
        /// <summary>
        /// For search conditions.<br />
        /// About attributes, use GenerateLobbyAttribute to set.
        /// </summary>
        /// <param name="bucket">important condition like game-mode, region, map</param>
        /// <param name="MaxPlayers">Max number of user allowed in the lobby</param>
        /// <returns></returns>
        public static Lobby GenerateLobbyObject(string[] bucket, uint MaxPlayers = 2){
            Lobby lobby = new Lobby();
            lobby.SetBucketID(bucket);
            lobby.MaxLobbyMembers = MaxPlayers;

            return lobby;
        }
        /// <summary>
        /// Change State text
        /// </summary>
        /// <param name="state"></param>
        internal void UpdateStateDescription(MatchState state){
            if(matchState.state == null){
                return;
            }
            matchState.state.text = matchState.GetDiscription(state);
        }

    #region Obsolete
        [Obsolete("This is old. ReconnecLobby() is new one.")]
        public async UniTask<bool> ReconnectParticipatingLobby(string LobbyID, CancellationTokenSource token){
            return await ReconnectLobby(LobbyID, token);
        }
        /// <summary>
        /// For search conditions.<br />
        /// About attributes, use GenerateLobbyAttribute to set.
        /// </summary>
        /// <param name="mode">For BucletID</param>
        /// <param name="region">For BucletID</param>
        /// <param name="mapName">For BucletID</param>
        /// <returns></returns>
        [Obsolete("This is old. GenerateLobbyObject(string[] bucket, uint MaxPlayers, bool bPresenceEnabled) is new one.")]
        public static Lobby GenerateLobby(string mode = "", string region = "",
                                            string mapName = "", uint MaxPlayers = 2,
                                            bool bPresenceEnabled = false){
            Lobby lobby = new Lobby();
            lobby.SetBucketID(new string[3]{ mode, region, mapName });
            lobby.MaxLobbyMembers = MaxPlayers;

            return lobby;
        }
    #endregion
        [Obsolete("This is old. GenerateLobbyObject(string[] bucket, uint MaxPlayers, bool bPresenceEnabled) is new one.")]
        public static Lobby GenerateLobby(string[] bucket, uint MaxPlayers = 2,
                                            bool bPresenceEnabled = false){
            Lobby lobby = new Lobby();
            lobby.SetBucketID(bucket);
            lobby.MaxLobbyMembers = MaxPlayers;

            return lobby;
        }

        [Obsolete("MatchMakeManager.Instance.lobbyIDMethod.Register(Action save, Action delete, bool changeType = true) is new one")]
        public void RegisterLobbyIDFunctions(Action save, Action delete, bool changeType = true){
            lobbyIDMethod.Register(save, delete, changeType);
        }
        [Obsolete("MatchMakeManager.Instance.asyncLobbyIDMethod.Register(Func<UniTask> save, Func<UniTask> delete, bool changeType = true is new one")]
        public void RegisterAsyncLobbyIDFunctions(Func<UniTask> save, Func<UniTask> delete, bool changeType = true){
            asyncLobbyIDMethod.Register(save, delete, changeType);
        }
    #endregion
    }
}
