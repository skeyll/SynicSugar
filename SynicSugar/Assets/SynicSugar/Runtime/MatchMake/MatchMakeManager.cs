using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

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
        }
        void OnDestroy() {
            if( Instance == this ) {
                Instance = null;
            }
        }
#endregion
        //Option
        [SerializeField] uint maxSearchResult = 5;
        [SerializeField] int hostsTimeoutSec = 180;
        public bool AllowUserReconnect = true;

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
        /// MatchMake player with conditions and get the data for p2p connect. <br />
        /// Search a lobby, then if can't join, create a lobby as host.
        /// </summary>
        /// <param name="lobbyCondition">Crate by EOSLobbyExtenstions.GenerateLobby().</param>
        /// <param name="token">To cancel task</param>
        /// <param name="saveFn">To save lobby ID for recconect. If null, use Playerprefs</param>
        /// <returns></returns>
        public async UniTask<bool> SearchAndCreateLobby(Lobby lobbyCondition, CancellationTokenSource token, Action saveFn = null ){
            //Match at Lobby
            bool canMatch = await eosLobby.StartMatching(lobbyCondition, token, saveFn);
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
        /// <param name="saveFn">To save lobby ID for recconect. If null, use Playerprefs</param>
        /// <returns></returns>
        public async UniTask<bool> SearchLobby(Lobby lobbyCondition, CancellationTokenSource token, Action saveFn = null ){
            //Match at Lobby
            bool canMatch = await eosLobby.StartJustSearch(lobbyCondition, token, saveFn);
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
        /// <param name="saveFn">To save lobby ID for recconect. If null, use Playerprefs</param>
        /// <returns></returns>
        public async UniTask<bool> CreateLobby(Lobby lobbyCondition, CancellationTokenSource token, Action saveFn = null ){
            //Match at Lobby
            bool canMatch = await eosLobby.StartJustCreate(lobbyCondition, token, saveFn);
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
        /// <summary>
        /// On the end of match and stopping match make, call this.
        /// 
        /// </summary>
        /// <param name="token"></param>
        /// <param name="deleteFn">Delete LobbyID for reconnect. If null, use Playerprefs</param>
        /// <returns>True on success. If user isn't host, return false.</returns>
        public async UniTask<bool> DestroyHostingLobby(CancellationTokenSource token, Action deleteFn = null){
            bool canDestroy = await eosLobby.DestroyLobby(token, deleteFn);

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
