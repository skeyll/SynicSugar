#pragma warning disable CS0414 //The field is assigned but its value is never used
using System;

namespace SynicSugar.P2P {
    /// <summary>
    /// Manage the notify around p2p connection.
    /// </summary>
    public class ConnectionNotifier {
        internal Reason ClosedReason { get; private set; }
        internal UserId CloseUserId { get; private set; }
        internal UserId ConnectUserId { get; private set; }

        /// <summary>
        /// Invoke when another user leaves.<br />
        /// </summary>
        public event Action<UserId> OnTargetLeaved;
        /// <summary>
        /// Invoke when another user disconnects unexpectedly.<br />
        /// This has a lag of about 5-10 seconds after a user downs in its local.
        /// </summary>
        public event Action<UserId> OnTargetDisconnected;
        /// <summary>
        /// Invoke when a user re-connects after matchmaking.<br />
        /// For returnee and newcomer
        /// </summary>
        public event Action<UserId> OnTargetConnected;
        
        /// <summary>
        /// Invoke when a connection is interrupted with another peer. <br />
        /// The connection is attempted to be restored, and if that's failed, "Diconnected" is fired.<br />
        /// This notification is early, but this doesn't means just that other user is disconnected.
        /// </summary>
        public event Action<UserId> OnTargetEarlyDisconnected;
        
        /// <summary>
        /// Invoke when a connection is restored with another EarlyDisconnected peer. <br />
        /// About game data, the peer should have it.
        /// </summary>
        public event Action<UserId> OnTargetRestored;
        
        /// <summary>
        /// Invoked when the Lobby is closed, and the local user is removed from it.<br />
        /// This can occur for one of the following reasons: <br />
        /// - Disconnected: An unexpected disconnection occurred.<br />
        /// - LobbyClosed: The Lobby was closed by the host.<br />
        /// - Kicked: The local user was removed from the Lobby.<br />
        /// LobbyID is deleted only when the Lobby is LobbyClosed.　If disconnected, to resume communication, rejoin with MatchMaking.Instance.ReconnectLobby().
        /// </summary>
        public event Action<Reason> OnLobbyClosed;

        public void Register(Action<UserId> leaved, Action<UserId> disconnected, Action<UserId> connected){
            OnTargetLeaved += leaved;
            OnTargetDisconnected += disconnected;
            OnTargetConnected += connected;
        }
        public void Register(Action<UserId> disconnected, Action<UserId> connected, Action<UserId> earlyDisconnected, Action<UserId> restored){
            OnTargetDisconnected += disconnected;
            OnTargetConnected += connected;
            OnTargetEarlyDisconnected += earlyDisconnected;
            OnTargetRestored += restored;
        }
        internal void Clear(){
            OnTargetLeaved = null;
            OnTargetDisconnected = null;
            OnTargetConnected = null;
            OnTargetEarlyDisconnected = null;
            OnTargetRestored = null;
            OnLobbyClosed = null;
            establishedMemberCounts = 0;
            completeConnectPreparetion = false;
        }
        /// <summary>
        /// Invoked when someone leaves the lobby for reasons other than Leave.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="reason"></param>
        internal void Disconnected(UserId id, Reason reason){
            ClosedReason = reason;
            CloseUserId = id;
            OnTargetDisconnected?.Invoke(id);
        }
        /// <summary>
        /// Invoked when someone come back to the lobby.
        /// </summary>
        /// <param name="id"></param>
        internal void Connected(UserId id){
            ConnectUserId = id;
            OnTargetConnected?.Invoke(id);
        }
        /// <summary>
        /// For AddNotifyPeerConnectionInterrupted
        /// </summary>
        /// <param name="id"></param>
        /// <param name="reason"></param>
        internal void EarlyDisconnected(UserId id, Reason reason){
            ClosedReason = reason;
            CloseUserId = id;
            OnTargetEarlyDisconnected?.Invoke(id);
        }
        /// <summary>
        /// For AddNotifyPeerConnectionInterrupted and Restored
        /// </summary>
        /// <param name="id"></param>
        internal void Restored(UserId id){
            ConnectUserId = id;
            OnTargetRestored?.Invoke(id);
        }
        /// <summary>
        /// Target user leaved from Lobby by SynicSugarAPI.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="reason"></param>
        internal void Leaved(UserId id, Reason reason){
            ClosedReason = reason;
            CloseUserId = id;
            OnTargetLeaved?.Invoke(id);
        }
        
        /// <summary>
        /// This Local user can't connect to lobby anymore.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="reason"></param>
        internal void Closed(Reason reason){
            ClosedReason = reason;
            OnLobbyClosed?.Invoke(reason);
        }
        private int establishedMemberCounts;
        internal bool completeConnectPreparetion; 
        internal void OnEstablished(){
            establishedMemberCounts++;
            completeConnectPreparetion = p2pInfo.Instance.userIds.RemoteUserIds.Count == establishedMemberCounts;
        }
    }
}