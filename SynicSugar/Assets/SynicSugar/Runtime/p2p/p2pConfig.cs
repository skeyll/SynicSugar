using Epic.OnlineServices.P2P;
using UnityEngine;
using System;
namespace SynicSugar.P2P {
    public class p2pConfig : MonoBehaviour {
#region Singleton
        private p2pConfig(){}
        public static p2pConfig Instance { get; private set; }
        void Awake() {
            if( Instance != null ) {
                Destroy( this );
                return;
            }
            Instance = this;
            AllowDelayedDelivery = FirstConnection == FirstConnectionType.TempDelayedDelivery || FirstConnection == FirstConnectionType.DelayedDelivery;
        }
        void OnDestroy() {
            if( Instance == this ) {
                Instance = null;
            }
        }
#endregion
        [Obsolete("p2pInfo.Instance.ConnectionNotifier is new one.")]
        public ConnectionNotifier ConnectionNotifier => p2pInfo.Instance.ConnectionNotifier;
        ///Options 
        [Header("Interval of sending each users[ms]. Recommend: 3ms-")]
        /// <summary>
        /// Interval ms on sending to each user in Rpc. <br />>
        /// If interval is too short and the sending buffer becomes full, the next packets will be discarded.<br />
        /// Recommend: 3ms-
        /// </summary>
        public int interval_sendToAll = 3;
        [Header("Interval until sending next new value[ms]. Recommend: 1000-3000ms.")]
        /// <summary>
        /// Interval ms that a SyncVar dosen't been send even if the value changes after send that SyncVar.<br />
        /// If set short value, may get congesting the band.<br />
        /// Recommend: 1000-3000ms.
        /// </summary>
        public int autoSyncInterval = 1000;
        /// <summary>
        /// Quality of connection
        /// </summary>
        public PacketReliability packetReliability = PacketReliability.ReliableOrdered;
        
        public enum GetPacketFrequency {
            PerSecond3xFPS, PerSecondFPS, PerSecond100, PerSecond50, PerSecond25
        }
        [Header("PacketReceiver's Frequency/per seconds *Never more than game FPS.")]
        /// <summary>
        /// Frequency of calling PacketReceiver.<br />
        /// Cannot exceed the recive's fps of the app's. <br />
        /// </summary>
        public GetPacketFrequency getPacketFrequency = GetPacketFrequency.PerSecond50;
        public bool UseDisconnectedEarlyNotify;
        /// <summary>
        /// Delay time to return true after matchmaking.<br />
        /// After the matchmaking is established, EOS need to request and accept connections with each other. This is the setting of how to handle it.
        /// </summary>
        public enum FirstConnectionType{
            /// <summary>
            /// Return true after having connected with all peers. This is reliable but need a time.
            /// </summary>
            Strict, 
            /// <summary>
            /// Return true after just sending connect request. Other peers will discard the initial some packets that the user sends during about 1-2sec after getting true. (Depends on the ping)
            /// </summary>
            Casual, 
            /// <summary>
            /// Return true after just sending connect request. Packets in 10 sec after matching are stored in the receive buffer even if the peer haven't accept the connection.
            /// </summary>
            TempDelayedDelivery, 
            /// <summary>
            /// Return true after just sending connect request. All packets are stored in the receive buffer even if the peer haven't accept the connection. PauseConnections() stops the work on this type.
            /// </summary>
            DelayedDelivery
        }
        
        /// <summary>
        /// Delay time to return true after matchmaking.<br />
        /// After the connection is established, EOS has a lag before actual communication is possible.  This is the setting of how to handle it.<br />
        /// </summary>
        public FirstConnectionType FirstConnection;
        /// <summary>
        /// MEMO: Can't change this in game for performance now.
        /// </summary>
        internal bool AllowDelayedDelivery;
        
        [Range(1, 4)]
        public byte SamplesPerPing;
        [Header("If false, need call RefreshPing to GetPing.")]
        public bool AutoRefreshPing;
        [Range(1, 60)]
        public int PingAutoRefreshRateSec = 10;
    }
}