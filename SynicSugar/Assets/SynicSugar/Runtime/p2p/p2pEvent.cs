#pragma warning disable CS0414 //The field is assigned but its value is never used
using System;
using System.Collections.Generic;

namespace SynicSugar.P2P {
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
            establishedMemberCounts = 0;
            completeConnectPreparetion = false;
        }
        internal void Disconnected(UserId id, Reason reason){
            ClosedReason = reason;
            CloseUserId = id;
            OnTargetDisconnected?.Invoke(id);
        }
        internal void Connected(UserId id){
            ConnectUserId = id;
            OnTargetConnected?.Invoke(id);
        }
        
        internal void EarlyDisconnected(UserId id, Reason reason){
            ClosedReason = reason;
            CloseUserId = id;
            OnTargetEarlyDisconnected?.Invoke(id);
        }
        internal void Restored(UserId id){
            ConnectUserId = id;
            OnTargetRestored?.Invoke(id);
        }
        internal void Leaved(UserId id, Reason reason){
            ClosedReason = reason;
            CloseUserId = id;
            OnTargetLeaved?.Invoke(id);
        }
        private int establishedMemberCounts;
        internal bool completeConnectPreparetion; 
        internal void OnEstablished(){
            establishedMemberCounts++;
            completeConnectPreparetion = p2pInfo.Instance.userIds.RemoteUserIds.Count == establishedMemberCounts;
        }
    }
    
    public class SyncSnyicNotifier {
        /// <summary>
        /// Invoke when Synic variables is synced.
        /// </summary>
        public event Action OnSyncedSynic;
        
        public void Register(Action syncedSynic){
            OnSyncedSynic += syncedSynic;
        }
        internal void Clear(){
            OnSyncedSynic = null;
        }

        internal UserId LastSyncedUserId { get; private set; }
        internal byte LastSyncedPhase { get; private set; }
        bool _receivedAllSyncSynic;
        List<string> ReceivedUsers = new List<string>();
        internal bool ReceivedAllSyncSynic(){
            if(_receivedAllSyncSynic){
                //Init
                ReceivedUsers.Clear();
                _receivedAllSyncSynic = false;

                return true;
            }
            return false;
        }
        //Access this from public method in p2pAssembleXXX.　We can move this to that for calling cast.
        internal void UpdateSyncedState(string id, byte phase){
            if (!ReceivedUsers.Contains(id)){
                ReceivedUsers.Add(id);
                LastSyncedUserId = UserId.GetUserId(id);
                LastSyncedPhase = phase;
            }

            OnSyncedSynic?.Invoke();

            if(ReceivedUsers.Count == p2pInfo.Instance.GetCurrentConnectionMemberCount()){
                _receivedAllSyncSynic = true;
            }
        }
    }
}