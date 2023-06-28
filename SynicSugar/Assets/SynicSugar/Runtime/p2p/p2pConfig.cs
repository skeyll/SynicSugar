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
        }
        void OnDestroy() {
            if( Instance == this ) {
                Instance = null;
            }
        }
#endregion
        [HideInInspector] public UserIds userIds = new UserIds();
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
        public enum ReceiveInterval{
            Large, Moderate, Small
        }
        [Header("delay[ms]/per recive:Small 10, Mod 25, Large 50")]
        /// <summary>
        /// Frequency of calling PacketReceiver. [Small 10ms, Moderate 25ms, Large 50ms]</ br>
        /// Cannot exceed the recive's fps of the app's. </ br>
        /// Recommend: Moderate. (-8peers, mobile game, non large-party acion game)
        /// </summary>
        public ReceiveInterval receiveInterval = ReceiveInterval.Moderate;
        /// <summary>
        /// Quality of connection
        /// </summary>
        public PacketReliability packetReliability = PacketReliability.ReliableOrdered;
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