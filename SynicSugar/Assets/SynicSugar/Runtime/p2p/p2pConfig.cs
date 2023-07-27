using Epic.OnlineServices.P2P;
using UnityEngine;
using System;
using System.Collections.Generic;
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
        }
        void OnDestroy() {
            if( Instance == this ) {
                ConnectionNotifier.Clear();

                Instance = null;
            }
        }
#endregion
        [HideInInspector] public UserIds userIds = new UserIds();
        public UserId LocalUserId => userIds.LocalUserId;
        public List<UserId> RemoteUserIds => userIds.RemoteUserIds;
        public bool AcceptHostSynic () => userIds._AcceptHostSynic();
        /// <summary>
        /// Is this local user Game Host?
        /// </summary>
        /// <returns></returns>
        public bool IsHost (){
            return userIds.LocalUserId == userIds.HostUserId;
        }
        /// <summary>
        /// Is this user Game Host?
        /// </summary>
        /// <returns></returns>
        public bool IsHost (UserId targetId){
            return targetId == userIds.HostUserId;
        }

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
        public ConnectionNotifier ConnectionNotifier = new ConnectionNotifier();
        
        public enum GetPacketFrequency {
            PerSecondFPS, PerSecond100, PerSecond50, PerSecond25
        }
        [Header("PacketReceiver's Frequency/per seconds *Never more than game FPS.")]
        /// <summary>
        /// Frequency of calling PacketReceiver.</ br>
        /// Cannot exceed the recive's fps of the app's. </ br>
        /// </summary>
        public GetPacketFrequency getPacketFrequency = GetPacketFrequency.PerSecond50;

    #region Obolete
        public enum ReceiveInterval{
            Large, Moderate, Small
        }
        [Obsolete("getPacketFrequency is new one"), HideInInspector]
        public ReceiveInterval receiveInterval { 
            get {
                switch(getPacketFrequency){
                    case GetPacketFrequency.PerSecond100:
                    return ReceiveInterval.Small;
                    case GetPacketFrequency.PerSecond50:
                    return ReceiveInterval.Moderate;
                    default:
                    return ReceiveInterval.Large;
                }
            }
            set {
                switch(value){
                    case ReceiveInterval.Large:
                    getPacketFrequency = GetPacketFrequency.PerSecond25;
                    break;
                    case ReceiveInterval.Moderate:
                    getPacketFrequency = GetPacketFrequency.PerSecond50;
                    break;
                    default:
                    getPacketFrequency = GetPacketFrequency.PerSecond100;
                    break;
                }
            }
        }
    #endregion
    }
#region Obsolete
    public class p2pManager {
        private p2pManager(){}
        [Obsolete("This is old. You can use p2pConfig.Instance.XXX().")]
        public static p2pManager Instance { get; private set; }
        
        [Obsolete("This is old. You can use p2pConfig.Instance.XXX")]
        [HideInInspector] public UserIds userIds {
            get{
                return p2pConfig.Instance.userIds;
            } 
            set { p2pConfig.Instance.userIds = value; }
        }
        
        [Obsolete("This is old. You can use p2pConfig.Instance.XXX")]
        public int interval_sendToAll {
            get{
                return p2pConfig.Instance.interval_sendToAll;
            } 
            set { p2pConfig.Instance.interval_sendToAll = value; }
        }

        [Obsolete("This is old. You can use p2pConfig.Instance.XXX")]
        public int autoSyncInterval{
            get{
                return p2pConfig.Instance.autoSyncInterval;
            } 
            set { p2pConfig.Instance.autoSyncInterval = value; }
        }
        public enum ReceiveInterval{
            Large, Moderate, Small
        }
        
        [Obsolete("This is old. You can use p2pConfig.Instance.XXX")]
        /// <summary>
        /// Frequency of calling PacketReceiver. [Small 10ms, Moderate 25ms, Large 50ms]</ br>
        /// Cannot exceed the recive's fps of the app's. </ br>
        /// Recommend: Moderate. (-8peers, mobile game, non large-party acion game)
        /// </summary>
        public ReceiveInterval receiveInterval = ReceiveInterval.Moderate;
        /// <summary>
        /// Quality of connection
        /// </summary>
        /// 
        [Obsolete("This is old. You can use p2pConfig.Instance.XXX")]
        public PacketReliability packetReliability = PacketReliability.ReliableOrdered;
    }
#endregion
}