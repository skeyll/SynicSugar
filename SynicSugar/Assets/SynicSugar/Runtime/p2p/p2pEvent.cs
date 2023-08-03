using System;

namespace SynicSugar.P2P {
    public class ConnectionNotifier {
        internal Reason ClosedReason { get; private set; }
        internal UserId CloseUserId { get; private set; }
        internal UserId ConnectUserId { get; private set; }

        /// <summary>
        /// Invoke when another user disconnects unexpectedly.</ br>
        /// This has a lag of about 5-10 seconds after a user downs in its local.
        /// </summary>
        public event Action Disconnected;
        /// <summary>
        /// Invoke when a user re-connects after matchmaking.</ br>
        /// For returnee and newcomer
        /// </summary>
        public event Action Connected;
        
        /// <summary>
        /// Invoke when a connection is interrupted with another peer. </ br>
        /// The connection is attempted to be restored, and if that's failed, "Diconnected" is fired.</ br>
        /// This notification is early, but this doesn't means just that other user is disconnected.
        /// </summary>
        public event Action EarlyDisconnected;
        
        /// <summary>
        /// Invoke when a connection is restored with another EarlyDisconnected peer. </ br>
        /// About game data, the peer should have it.
        /// </summary>
        public event Action Restored;

        public void Register(Action disconnected, Action connected){
            Disconnected += disconnected;
            Connected += connected;
        }
        public void Register(Action disconnected, Action connected, Action earlyDisconnected, Action restored){
            Disconnected += disconnected;
            Connected += connected;
            EarlyDisconnected += earlyDisconnected;
            Restored += restored;
        }
        internal void Clear(){
            Disconnected = null;
            Connected = null;
            EarlyDisconnected = null;
            Restored = null;
        }
        internal void OnDisconnected(UserId id, Reason reason){
            ClosedReason = reason;
            CloseUserId = id;
            Disconnected?.Invoke();
        }
        internal void OnConnected(UserId id){
            ConnectUserId = id;
            Connected?.Invoke();
        }
        
        internal void OnEarlyDisconnected(UserId id, Reason reason){
            ClosedReason = reason;
            CloseUserId = id;
            EarlyDisconnected?.Invoke();
        }
        internal void OnRestored(UserId id){
            ConnectUserId = id;
            Connected?.Invoke();
        }
    }

}