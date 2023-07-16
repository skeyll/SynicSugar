using System;

namespace SynicSugar.P2P {
    public class ConnectionNotifier {
        public Reason ClosedReason { get; private set; }
        public UserId TargetUserId { get; private set; }

        /// <summary>
        /// Invoke when another user disconnects unexpectedly.</ br>
        /// This has a lag of about 5-10 seconds after a user downs in its local.
        /// </summary>
        public event Action Disconnected;
        /// <summary>
        /// Invoke when a user connects after matchmaking.</ br>
        /// For returnee and newcomer
        /// </summary>
        public event Action Connected;

        public void Register(Action disconnected, Action connected){
            Disconnected += disconnected;
            Connected += connected;
        }
        internal void Clear(){
            Disconnected = null;
            Connected = null;
        }
        internal void OnDisconnected(UserId id, Reason reason){
            ClosedReason = reason;
            TargetUserId = id;
            Disconnected?.Invoke();
        }
        internal void OnConnected(UserId id){
            TargetUserId = id;
            Connected?.Invoke();
        }
    }

}