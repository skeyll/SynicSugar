using Epic.OnlineServices.P2P;
using UnityEngine;
using System;
namespace SynicSugar.P2P {
    public class p2pConfig : MonoBehaviour {
#region Singleton
        public static p2pConfig Instance { get; private set; }
        void Awake() {
            if( Instance != null ) {
                Destroy( this );
                return;
            }
            Instance = this;
            natRelay = new NatRelayManager();
            p2pConnectorForOtherAssembly.Instance.InitConencter();
        }
        void OnDestroy() {
            if( Instance == this ) {
                p2pConnectorForOtherAssembly.Instance.Dispose();
                Instance = null;
            }
        }
#endregion
        NatRelayManager natRelay;
        /// <summary>
        /// Users with settings NoRelay and ForceRelays cannot connect.<br />
        /// So, we should use only AllowRelays and one of the other settings.<br /?
        /// AllowRelay is default. In default, if the connection can be made via p2p, users connect directly; if it fails NAT Punch through, users use Relay(AWS) for the connection.<br />
        /// If we set this 
        /// </summary>
        public RelayControl relayControl = RelayControl.AllowRelays;
        ///Options 
        /// <summary>
        /// Quality of connection
        /// </summary>
        public PacketReliability packetReliability = PacketReliability.ReliableOrdered;

        /// <summary>
        /// Which the packet to reach the users who have not yet got a connection or not.<br />
        /// If True, PausePacketReceiver, etc. is not be available.<br />
        /// MEMO: Can't change this in game for performance now.
        /// </summary>
        public bool AllowDelayedDelivery = false;
        public bool UseDisconnectedEarlyNotify;

        public enum GetPacketFrequency {
            PerSecondBurstFPS, PerSecondFPS, PerSecond100, PerSecond50, PerSecond25
        }
        [HideInInspector, Obsolete("This will soon be obsolete. This is managed from PacketReciveTiming that is in PacketReceiver's args now.")] 
        /// <summary>
        /// PacketReceiver's Frequency/per seconds.<br />
        /// Cannot exceed the recive's fps of the app's. <br />
        /// </summary>
        public GetPacketFrequency getPacketFrequency = GetPacketFrequency.PerSecond50;
        /// <summary>
        /// Frequency of BurstFPS's GetPacket in a frame. Recommend: 2-5
        /// </summary>
        [HideInInspector, Obsolete("This will soon be obsolete. This is managed from PacketReciveTiming that is in PacketReceiver's args now.")]
        public int BurstReceiveBatchSize = 5;
        [Space(10), Range(1, 16)]
        /// <summary>
        /// The number of target users to be sent packet of RPC in a frame. Wait for a frame after a set. <br />
        /// The sending buffer is probably around 64 KB, so it should not exceed this. If we set 0 from the script, it will cause crash.
        /// </summary>
        public int RPCBatchSize = 3;
        [Range(1, 16)]
        /// <summary>
        /// The number of packets to be sent of a large packet in a frame. Wait for a frame after a set. <br />
        /// The sending buffer is probably around 64 KB, so it should not exceed this. If we set 0 from the script, it will cause crash.
        /// </summary>
        public int LargePacketBatchSize = 3;
        /// <summary>
        /// Frequency of GetSynicPacket in a frame. Recommend: 5-8
        /// </summary>
        [Range(2, 16)]
        public int SynicReceiverBatchSize = 5;
        [Range(0, 5000)]
        /// <summary>
        /// Interval ms that a SyncVar dosen't been send even if the value changes after send that SyncVar.<br />
        /// If set short value, may get congesting the band.<br />
        /// Recommend: 1000-3000ms.
        /// </summary>
        public int autoSyncInterval = 1000;

        /// <summary>
        /// False if ping is not needed. We can also refresh to call RefreshPing manually.
        /// </summary>
        [Space(10)] 
        public bool AutoRefreshPing;
        [Range(1, 4)]
        public byte SamplesPerPing;
        [Range(1, 60)]
        public int PingAutoRefreshRateSec = 10;

        /// <summary>
        /// Set how relay servers are to be used. This setting does not immediately apply to existing connections, but may apply to existing connections if the connection requires renegotiation.<br /> 
        /// AllowRelay is default. In default, if the connection can be made via p2p, users connect directly; if it fails NAT Punch through, users use Relay(AWS) for the connection.<br />
        /// If it is set to anything other than AllowRelays, SetRelayControl is automatically called before the first connection. If SetRelayControl() is called after the connection, connection will switch between via Relay and p2p when the connect is not stable, so it is better to change this value in the editor or just before or after matching starts.
        /// </summary>
        /// <param name="relay">AllowRelay is Default</param>
        public void SetRelayControl(RelayControl relay){
            natRelay.SetRelayControl(relay);
        }
    }
}