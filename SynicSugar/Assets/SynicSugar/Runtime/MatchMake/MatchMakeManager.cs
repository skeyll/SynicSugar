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

            eosLobby = new EOSLobby(maxSearchResult, hostsTimeoutSec, AllowUserReconnect);
            
            if(custom_method_save_lobbyId != null && custom_method_delete_lobbyId != null){
                eosLobby.RegisterLobbyIdEvent((() => custom_method_save_lobbyId.Invoke()), (() => custom_method_delete_lobbyId.Invoke()));
            }
        }
        void OnDestroy() {
            if( Instance == this ) {
                Instance = null;
            }
        }
#endregion
        //Option
        public uint maxSearchResult = 5;
        public int hostsTimeoutSec = 180;
    #region TODO: Change this to Enum and display only one field for the selected way.
        public bool AllowUserReconnect = true;
        [Header("Save and Delete must be pair. <br>We can also use RegisterLobbyIDFunctions(UnityEvent save, UnityEvent delete) to set.")]
        public UnityEvent custom_method_save_lobbyId;
        public UnityEvent custom_method_delete_lobbyId;
        [Header("If the above two aren't set, Playerprefs will be used by default.")]
        public string playerprefs_lobbyId_key = "eos_lobbyid";
    #endregion
        EOSLobby eosLobby;
        public MatchGUIState matchState = new MatchGUIState();

        /// <summary>
        /// Set State from script
        /// </summary>
        /// <param name="state"></param>
        public void SetGUIState(MatchGUIState state){
            matchState = state;
        }
        /// <summary>
        /// Register UnityEvent as Action to save and delete lobby Id for re-connect.<br />
        /// We can use cloud and save assets for this, but these place to be saved and deleted must be in the same place. <br />
        /// If it is not registered, it will be stored in the playerprefs with the key eos_lobbyid.
        /// </summary>
        /// <param name="save">void function</param>
        /// <param name="delete">void function</param>
        public void RegisterLobbyIDFunctions(UnityEvent save, UnityEvent delete){
            if(save == null || delete == null){
                Debug.LogError("RegisterLobbyIDFunctions: need both save and delete.");
                return;
            }
            eosLobby.RegisterLobbyIdEvent((() => save.Invoke()), (() => delete.Invoke()));
        }
        /// <summary>
        /// MatchMake player with conditions and get the data for p2p connect. <br />
        /// Search a lobby, then if can't join, create a lobby as host.
        /// </summary>
        /// <param name="lobbyCondition">Crate by EOSLobbyExtenstions.GenerateLobby().</param>
        /// <param name="token">To cancel task</param>
        /// <returns></returns>
        public async UniTask<bool> SearchAndCreateLobby(Lobby lobbyCondition, CancellationTokenSource token){
            //Match at Lobby
            bool canMatch = await eosLobby.StartMatching(lobbyCondition, token);

            if(token.IsCancellationRequested){
                return false;
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
        /// <param name="token">To cancel task</param>
        /// <returns></returns>
        public async UniTask<bool> SearchLobby(Lobby lobbyCondition, CancellationTokenSource token){
            //Match at Lobby
            bool canMatch = await eosLobby.StartJustSearch(lobbyCondition, token);
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
        /// <param name="token">To cancel task</param>
        /// <returns></returns>
        public async UniTask<bool> CreateLobby(Lobby lobbyCondition, CancellationTokenSource token){
            //Match at Lobby
            bool canMatch = await eosLobby.StartJustCreate(lobbyCondition, token);
            
            if(token.IsCancellationRequested){
                return false;
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
        public async UniTask<bool> ReconnectParticipatingLobby(string LobbyID, CancellationTokenSource token){
            if(string.IsNullOrEmpty(LobbyID)){
                return false;
            }

            bool canJoin = await eosLobby.ReconnectParticipatingLobby(LobbyID, token);

            return canJoin;
        }
        public string DeleteLobbyID(string key = ""){
            return PlayerPrefs.GetString ("eos_lobbyid", System.String.Empty);
        }
        /// <summary>
        /// Exit lobby and cancel MatchMake.
        /// </summary>
        /// <param name="token">token for this task</param>
        /// <returns></returns>
        public async UniTask<bool> CancelCurrentMatchMake(CancellationTokenSource token){
            return await eosLobby.CancelMatchMaking(token);
        }
        /// <summary>
        /// Leave the current lobby in Game.
        /// </summary>
        /// <param name="token"></param>
        internal async UniTask<bool> ExitCurrentLobby(CancellationTokenSource token){
            bool canDestroy = await eosLobby.LeaveLobby(false, token);

            return canDestroy;
        }
        /// <summary>
        /// Destroy the current lobby on the end of Game.
        /// </summary>
        /// <param name="token"></param>
        /// <returns>True on success. If user isn't host, return false.</returns>
        internal async UniTask<bool> CloseCurrentLobby(CancellationTokenSource token){
            //Register the action to delete LobbyID for reconnect on Start or Constructer.

            bool canDestroy = await eosLobby.DestroyLobby(token);

            return canDestroy;
        }
        /// <summary>
        /// For search conditions.<br />
        /// About attributes, use GenerateLobbyAttribute to set.
        /// </summary>
        /// <param name="mode">For BucletID</param>
        /// <param name="region">For BucletID</param>
        /// <param name="mapName">For BucletID</param>
        /// <returns></returns>
        [Obsolete]
        public static Lobby GenerateLobby(string mode = "", string region = "",
                                            string mapName = "", uint MaxPlayers = 2,
                                            bool bPresenceEnabled = false){
            Lobby lobby = new Lobby();
            lobby.SetBucketID(new string[3]{ mode, region, mapName });
            lobby.MaxLobbyMembers = MaxPlayers;
            lobby.bPresenceEnabled = bPresenceEnabled;

            return lobby;
        }
        /// <summary>
        /// For search conditions.<br />
        /// About attributes, use GenerateLobbyAttribute to set.
        /// </summary>
        /// <param name="bucket">important condition like mode, region, map name</param>
        /// <param name="MaxPlayers"></param>
        /// <param name="bPresenceEnabled"></param>
        /// <returns></returns>
        public static Lobby GenerateLobby(string[] bucket, uint MaxPlayers = 2,
                                            bool bPresenceEnabled = false){
            Lobby lobby = new Lobby();
            lobby.SetBucketID(bucket);
            lobby.MaxLobbyMembers = MaxPlayers;
            lobby.bPresenceEnabled = bPresenceEnabled;

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
    }
}
