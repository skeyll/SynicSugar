using System.Threading;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using SynicSugar.P2P;

namespace SynicSugar.MatchMake {
    public abstract class MatchmakingCore {
        protected uint MAX_SEARCH_RESULT;
        protected int timeoutMS, initconnectTimeoutMS;
        protected MatchmakingCore(uint maxSearch, int MatchmakingTimeout, int InitialConnectionTimeout){
            MAX_SEARCH_RESULT = maxSearch;
            //For Unitask
            timeoutMS = MatchmakingTimeout * 1000;
            initconnectTimeoutMS = InitialConnectionTimeout * 1000;
        }
        /// <summary>
        /// If call this in matchmaking, it could cause a bug.
        /// </summary>
        /// <param name="MatchmakingTimeout"></param>
        /// <param name="InitialConnectionTimeout"></param>
        public virtual void SetTimeoutSec(int MatchmakingTimeout, int InitialConnectionTimeout){
            //For Unitask
            timeoutMS = MatchmakingTimeout * 1000;
            initconnectTimeoutMS = InitialConnectionTimeout * 1000;
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
        public abstract UniTask<Result> StartMatching(Lobby lobbyCondition, CancellationToken token, List<AttributeData> userAttributes, uint minLobbyMember);

        /// <summary>
        /// Just search Lobby<br />
        /// Recommend: StartMatching()
        /// </summary>
        /// <param name="lobbyCondition">Search condition. <c>MUST NOT</c> add the data not to open public.</param>
        /// <param name="token"></param>
        /// <param name="userAttributes"></param>
        /// <param name="minLobbyMember"></param>
        /// <returns>True on success. If false, EOS backend have something problem. So, when you call this process again, should wait for some time.</returns>
        public abstract UniTask<Result> StartJustSearch(Lobby lobbyCondition, CancellationToken token, List<AttributeData> userAttributes, uint minLobbyMember);
    
        /// <summary>
        /// Create lobby as host<br />
        /// Recommend: StartMatching()
        /// </summary>
        /// <param name="lobbyCondition">Create and search condition. <c>MUST NOT</c> add the data not to open public.</param>
        /// <param name="token"></param>
        /// <param name="userAttributes"></param>
        /// <param name="minLobbyMember"></param>
        /// <returns>True on success. If false, EOS backend have something problem. So, when you call this process again, should wait for some time.</returns>
        public abstract UniTask<Result> StartJustCreate(Lobby lobbyCondition, CancellationToken token, List<AttributeData> userAttributes, uint minLobbyMember);
    

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
        /// <param name="matchmakeTokenSource"></param>
        /// <param name="cleanupMemberCountChanged"></param>
        /// <param name="token"></param>
        /// <returns>If true, user can leave or destroy the lobby. </returns>
        public abstract UniTask<Result> CancelMatchMaking(CancellationTokenSource matchmakeTokenSource, bool cleanupMemberCountChanged = false, CancellationToken token = default(CancellationToken));
        
        /// <summary>
        /// Host close matchmaking. Guest Cancel matchmaking.
        /// </summary>
        /// <param name="matchmakeTokenSource"></param>
        /// <param name="cleanupMemberCountChanged"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public abstract UniTask<Result> CloseMatchMaking(CancellationTokenSource matchmakeTokenSource, bool cleanupMemberCountChanged = false, CancellationToken token = default(CancellationToken));
       
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
        /// <param name="cleanupMemberCountChanged"></param>
        /// <param name="token"></param>
        public abstract UniTask<Result> LeaveLobby(bool cleanupMemberCountChanged = false, CancellationToken token = default(CancellationToken));

        /// <summary>
        /// When a game is over, call this. Guest leaves Lobby by update notify.
        /// </summary>
        /// <param name="cleanupMemberCountChanged"></param>
        /// <param name="token"></param>
        /// <returns>On destroy success, return true.</returns>
        public abstract UniTask<Result> DestroyLobby(bool cleanupMemberCountChanged = false, CancellationToken token = default(CancellationToken));
        
        /// <summary>
        /// Search for lobbies in backend and join in one to meet conditions.<br />
        /// When player could not join, they create lobby as host and wait for other player.
        /// </summary>
        /// <param name="lobbyCondition">Create and search condition. <c>MUST NOT</c> add the data not to open public.</param>
        /// <param name="delay"></param>
        /// <param name="userAttributes"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public abstract UniTask<Result> CreateOfflineLobby(Lobby lobbyCondition, OfflineMatchmakingDelay delay, List<AttributeData> userAttributes, CancellationToken token);
        
        public abstract UniTask<Result> DestroyOfflineLobby();

        public abstract bool isHost { get; }
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
    }
}

