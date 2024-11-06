using System.Threading;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using SynicSugar.MatchMake;
using SynicSugar.P2P;

namespace SynicSugar.Base {
    public abstract class MatchmakingCore {
        protected uint MAX_SEARCH_RESULT;
        protected MatchmakingCore(uint maxSearch){
            MAX_SEARCH_RESULT = maxSearch;
        }

        /// <summary>
        /// Search for lobbies in backend and join in one to meet conditions.<br />
        /// When player could not join, they create lobby as host and wait for other player.
        /// </summary>
        /// <param name="lobbyCondition">Create and search condition. <c>MUST NOT</c> add the data not to open public.</param>
        /// <param name="token"></param>
        /// <param name="userAttributes"></param>
        /// <param name="minLobbyMember"></param>
        /// <returns></returns>
        public abstract UniTask<Result> StartMatching(Lobby lobbyCondition, List<AttributeData> userAttributes, uint minLobbyMember, CancellationToken token);

        /// <summary>
        /// Just search Lobby<br />
        /// Recommend: StartMatching()
        /// </summary>
        /// <param name="lobbyCondition">Search condition. <c>MUST NOT</c> add the data not to open public.</param>
        /// <param name="token"></param>
        /// <param name="userAttributes"></param>
        /// <param name="minLobbyMember"></param>
        /// <returns>True on success. If false, EOS backend have something problem. So, when you call this process again, should wait for some time.</returns>
        public abstract UniTask<Result> StartJustSearch(Lobby lobbyCondition, List<AttributeData> userAttributes, uint minLobbyMember, CancellationToken token);
    
        /// <summary>
        /// Create lobby as host<br />
        /// Recommend: StartMatching()
        /// </summary>
        /// <param name="lobbyCondition">Create and search condition. <c>MUST NOT</c> add the data not to open public.</param>
        /// <param name="token"></param>
        /// <param name="userAttributes"></param>
        /// <param name="minLobbyMember"></param>
        /// <returns>True on success. If false, EOS backend have something problem. So, when you call this process again, should wait for some time.</returns>
        public abstract UniTask<Result> StartJustCreate(Lobby lobbyCondition, List<AttributeData> userAttributes, uint minLobbyMember, CancellationToken token);
    

        /// <summary>
        /// Join the Lobby with specific id to that lobby. <br />
        /// To return to a disconnected lobby.
        /// </summary>
        /// <param name="LobbyID">Lobby ID to <c>re</c>-connect</param>
        /// <param name="token"></param>
        public abstract UniTask<Result> JoinLobbyBySavedLobbyId(string LobbyID, CancellationToken token);

        /// <summary>
        /// Add scoketName and remove search attributes.
        /// This process is the preparation for p2p connect and re-Connect. <br />
        /// Use lobbyID to connect on the problem, so save lobbyID in local somewhere.
        /// </summary>
        /// <returns></returns>
        public abstract void SwitchLobbyAttribute();

        /// <summary>
        /// Cancel MatcgMaking and leave the lobby.
        /// </summary>
        /// <param name="token"></param>
        /// <returns>If true, user can leave or destroy the lobby. </returns>
        public abstract UniTask<Result> CancelMatchMaking(CancellationToken token = default(CancellationToken));
        
        /// <summary>
        /// Host close matchmaking. Guest Cancel matchmaking.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public abstract UniTask<Result> CloseMatchMaking(CancellationToken token = default(CancellationToken));
       
        /// <summary>
        /// Currently preventing duplicate calls.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public abstract UniTask<Result> KickTargetMember(UserId target, CancellationToken token);
       
        /// <summary>
        /// Leave the Participating Lobby.<br />
        /// When a game is over, call DestroyLobby() instead of this.
        /// </summary>
        /// <param name="token"></param>
        public abstract UniTask<Result> LeaveLobby(CancellationToken token);

        /// <summary>
        /// When a game is over, call this. Guest leaves Lobby by update notify.
        /// </summary>
        /// <param name="token"></param>
        /// <returns>On destroy success, return true.</returns>
        public abstract UniTask<Result> DestroyLobby(CancellationToken token);
        
        /// <summary>
        /// Search for lobbies in backend and join in one to meet conditions.<br />
        /// When player could not join, they create lobby as host and wait for other player.
        /// </summary>
        /// <param name="lobbyCondition">Create and search condition. <c>MUST NOT</c> add the data not to open public.</param>
        /// <param name="delay">To create pseudo-delay</param>
        /// <param name="userAttributes"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public abstract UniTask<Result> CreateOfflineLobby(Lobby lobbyCondition, OfflineMatchmakingDelay delay, List<AttributeData> userAttributes, CancellationToken token);
        
        /// <summary>
        /// Destroy Offline lobby that has created by CreateOfflineLobby.
        /// </summary>
        /// <returns></returns>
        public abstract UniTask<Result> DestroyOfflineLobby(CancellationToken token);

        /// <summary>
        /// Calling after the opponents are found and the lobby is closed. Establish communication and exchange UserID lists, then return results when the user is ready to communicate.
        /// </summary>
        /// <param name="setupTimeoutSec">the time from starting to establishing the connections.</param>
        /// <param name="token">Token for this task</param>
        /// <returns></returns>
        public abstract UniTask<Result> SetupP2PConnection(ushort setupTimeoutSec, CancellationToken token);

        /// <summary>
        /// Return whether the local user is host or not.
        /// </summary>
        /// <returns></returns>
        public abstract bool isHost();
        /// <summary>
        /// Returns whether this userId is a local user or not.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public abstract bool isLocalUser(UserId userId);
        /// <summary>
        /// Get current lobby member count.
        /// </summary>
        /// <returns></returns>
        public abstract int GetCurrentLobbyMemberCount();

        /// <summary>
        /// Get lobby Capacity.
        /// </summary>
        /// <returns></returns>
        public abstract int GetLobbyMemberLimit();

        /// <summary>
        /// For library user to save ID.
        /// </summary>
        /// <returns></returns>
        public abstract string GetCurrentLobbyID();

        /// <summary>
        /// Get target attribute about Key.
        /// </summary>
        /// <param name="target">target user id</param>
        /// <param name="Key">attribute key</param>
        /// <returns>If no data, return null.</returns>
        public abstract AttributeData GetTargetAttributeData(UserId target, string Key);
        
        /// <summary>
        /// Get target all attributes.
        /// </summary>
        /// <param name="target">target user id</param>
        /// <returns>If no data, return null.</returns>
        public abstract List<AttributeData> GetTargetAttributeData(UserId target);
        
        /// <summary>
        /// To check disconencted user's conenction state after p2p.
        /// </summary>
        /// <param name="disconenctedUserIndex"> UserIndex. For second Heart beat, +100</param>
        public abstract void UpdateMemberAttributeAsHeartBeat(int disconenctedUserIndex);

        /// <summary>
        /// Cleans up and closes the active session when the Lobby is closed, either intentionally or due to network issues.<br />
        /// This method handles session disconnection and Initialize IsInSession.<br />
        /// If there is a possibility that the local user could be kicked from the Lobby by someone other than themselves during the session, please call this method to terminate the communication.
        /// </summary>
        /// <param name="reason">The reason for the　lobby closure</param>
        /// <returns>indicating the success or failure of the cleanup process.</returns>
        protected async UniTask<Result> CloseSessionOnLobbyClosure(Reason reason){
            Result result = p2pConfig.Instance.sessionCore.RemoveNotifyAndCloseConnection();

            if(result != Result.Success){
                UnityEngine.Debug.LogErrorFormat("OnLobbyMemberStatusReceived: Failed to process Lobby closure. Host functions and lobby notifications may now be disabled, but P2P remains active. Result: {0}", result);
                return result;
            }

            // If the reason is LobbyClosed, remove the Lobby ID. Otherwise, the local user can reconnect via Reconnect API.
            if(reason == Reason.LobbyClosed){
                await MatchMakeManager.Instance.OnDeleteLobbyID();
            }

            SynicSugarManger.Instance.State.IsInSession = false;

            p2pInfo.Instance.ConnectionNotifier.Closed(reason);
            return Result.Success;
        }
    }
}

