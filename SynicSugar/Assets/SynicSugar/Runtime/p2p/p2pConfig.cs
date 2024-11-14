using Epic.OnlineServices.P2P;
using UnityEngine;
using System;
using SynicSugar.Base;

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
            sessionCore = SynicSugarManger.Instance.CoreFactory.GetSessionCore();
            p2pInfo.Instance.SetDependency(sessionCore, natRelayManager);
        }
        void OnDestroy() {
            if( Instance == this ) {
                sessionCore.Dispose();
                Instance = null;
            }
        }
#endregion
        internal SessionCore sessionCore;
        internal readonly NatRelayManager natRelayManager = new NatRelayManager();
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
        /// <summary>
        /// If true, use NotifyPeerConnectionInterrupted. Events: EarlyDisconnected, Restored, Disconnected<br />
        /// When a connection with someone becomes unstable, an event is triggered and EarlyDisconnected is invoked.  <br />
        /// Someones(Host + 2 peers) in the lobby will check anyone has been disconnected by sending a heartbeat to the lobby. 
        /// A reconnection attempt will be made, and if connection is restored, Restored will be invoked.
        /// When that user's connection with the lobby has also been lost, Disconnected will be called.<br />
        /// If false, use　NotifyPeerConnectionClosed. Events: Disconnected<br />
        /// When a connection becomes unstable, attempts to reconnect are made in the back. <br />
        /// If reconnection fails, a heartbeat to the lobby is send to the lobby within p2p's NotifyPeerConnectionClosed. <br />
        /// When that user's connection with the lobby has also been lost, Disconnected will be called.
        /// </summary>
        public bool UseDisconnectedEarlyNotify;

        /// <summary>
        /// The number of target users to be sent packet of RPC in a frame. Wait for a frame after a set. <br />
        /// The sending buffer is probably around 64 KB, so it should not exceed this. If we set 0 from the script, it will cause crash.
        /// </summary>
        [Space(10), Range(1, 16)]
        public int RPCBatchSize = 3;
        /// <summary>
        /// The number of packets to be sent of a large packet in a frame. Wait for a frame after a set. <br />
        /// The sending buffer is probably around 64 KB, so it should not exceed this. If we set 0 from the script, it will cause crash.
        /// </summary>
        [Range(1, 16)]
        public int LargePacketBatchSize = 3;
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
            natRelayManager.SetRelayControl(relay);
        }
        /// <summary>
        /// Get instance to manage connection. <br />
        /// <b>Note:</b> It is strongly recommended to use NetworkCore via ConnectHub, as it manages essential connection processes
        /// and dependencies. Directly accessing this instance is allowed but should be done with caution, as improper handling may lead 
        /// to unexpected behavior or bugs.
        /// </summary>
        /// <returns>Instance of INetworkCore</returns>
        public INetworkCore GetNetworkCore(){
            return sessionCore;
        }
    }
}