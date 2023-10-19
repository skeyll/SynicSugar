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
        ///Options 
        [Range(1, 10), Header("The number of target users to be sent packet of RPC in a frame. Recommend: 1-5")]
        /// <summary>
        /// The number of target users to be sent packet of RPC in a frame. Wait for a frame after a set. <br />
        /// The sending buffer is probably around 64 KB, so it should not exceed this. If we set 0 from the script, it will cause crash.
        /// </summary>
        public int SendToAllBatchSize = 3;
        [Range(1, 10), Header("The number of packets to be sent of a large packet in a frame. Recommend: 1-5")]
        /// <summary>
        /// The number of packets to be sent of a large packet in a frame. Wait for a frame after a set. <br />
        /// The sending buffer is probably around 64 KB, so it should not exceed this. If we set 0 from the script, it will cause crash.
        /// </summary>
        public int SendLargePacketBatchSize = 3;

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
            PerSecondBurstFPS, PerSecondFPS, PerSecond100, PerSecond50, PerSecond25
        }
        [Range(2, 10), Header("The number of times to be get packet in a frame. Recommend: 2-5")]
        public int BurstReceiveBatchSize = 5;

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