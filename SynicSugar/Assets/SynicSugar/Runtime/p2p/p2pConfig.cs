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
            AllowDelayedDelivery = InitialConnection == InitialConnectionType.PacketBuffered;
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
        /// Interval ms on sending to each user in Rpc. </ br>
        /// If interval is too short and the sending buffer becomes full, the next packets will be discarded.</ br>
        /// Recommend: 3ms-
        /// </summary>
        public int interval_sendToAll = 3;
        [Header("Interval until sending next new value[ms]. Recommend: 1000-3000ms.")]
        /// <summary>
        /// Interval ms that a SyncVar dosen't been send even if the value changes after send that SyncVar.</ br>
        /// If set short value, may get congesting the band.</ br>
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
        /// Frequency of calling PacketReceiver.</ br>
        /// Cannot exceed the recive's fps of the app's. </ br>
        /// </summary>
        public GetPacketFrequency getPacketFrequency = GetPacketFrequency.PerSecond50;
        public bool UseDisconnectedEarlyNotify;
        public enum InitialConnectionType {
            Stable, PacketBuffered, Casual
        }
        /// <summary>
        /// Delay time to return true after matchmaking.</ br>
        /// Stable returns true after have finished all connection preparations completely (Up to 10 sec).</ br>
        /// PacketBuffered change the packets type to reaching buffering's. This presses on the receiving buffer and PauseConnections() will stop that work.</ br>
        /// Casual returns true after just send a connection request. Other peers will discard the initial some packets that the user sends during about 50-200ms after getting true. (Depends on the ping, and probably about 3fps)
        /// </summary>
        public InitialConnectionType InitialConnection;
        /// <summary>
        /// MEMO: Can't change this in game for performance now.
        /// </summary>
        internal bool AllowDelayedDelivery;
    }
}