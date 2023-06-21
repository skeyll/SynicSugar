using PlayEveryWare.EpicOnlineServices;
using Epic.OnlineServices;
using Epic.OnlineServices.P2P;
using System;
using System.Threading;
using UnityEngine;
//TODO: The no understanding of Assembly caused the snarky struct.
//Ideally, the library's API should be here.
//I can not use some method like ConnectHub in Assembly-CSharp from sub-Assembly, SynicSugar.dll.
namespace SynicSugar.P2P {
    public class p2pManager : MonoBehaviour {
#region Singleton
        private p2pManager(){}
        public static p2pManager Instance { get; private set; }
        void Awake() {
            if( Instance != null ) {
                Destroy( this.gameObject );
                return;
            }
            Instance = this;
            DontDestroyOnLoad(this);
            SetReciveDelay(receiveInterval);
        }
        void OnDestroy() {
            if( Instance == this ) {
                Instance = null;
            }
        }
        void Start(){
            P2PHandle = EOSManager.Instance.GetEOSPlatformInterface().GetP2PInterface();
        }
#endregion
        internal P2PInterface P2PHandle;
        [HideInInspector] public UserIds userIds = new UserIds();
        string _socketName;
        public string ScoketName { 
            get { return _socketName; }
            set {
                _socketName = value;
                SocketId = new SocketId(){ SocketName = _socketName };
            }}
        public SocketId SocketId { get; private set; }

        ulong RequestNotifyId;
        public CancellationTokenSource p2pToken;
        ///Options 
        
        [Header("Delay of sending all users[ms]. Recommend: 3ms-")]
        // If interval is too short that the sending buffer gets be full and the packet will be discarded.
        public int interval_sendToAll = 3;
        [Header("No send new value for a some time after the value has send.[ms]")]
        // Recommend: 1000-3000ms.
        // Even if set shorten, network has lag by ping. So use synced value to modify local calculations.
        public int AutoSyncInterval = 1000;
        public enum ReceiveInterval{
            Large, Moderate, Small
        }
        [Header("delay[ms]/per recive:Small 10, Mod 25, Large 50")]
        // Cannot exceed the recive's fps of the app's.
        // Recommend is Moderate. (-8peers, mobile game, non large-party acion game)
        public ReceiveInterval receiveInterval = ReceiveInterval.Moderate;
        public int delay_receive { get; private set; } = 25;
        public PacketReliability packetReliability = PacketReliability.ReliableOrdered;
        void SetReciveDelay(ReceiveInterval gap){
            if(gap == ReceiveInterval.Large){
                delay_receive = 50;
            }else if(gap == ReceiveInterval.Moderate){
                delay_receive = 25;
            }else{
                delay_receive = 10;
            }
        }
        #region obsolete. Delete in future
        [HideInInspector, Obsolete] public int delay_sendToAll = 3;
        [Obsolete]
        public enum ReceiveDelay{
            Large, Moderate, Small
        }
        [HideInInspector, Obsolete]
        public ReceiveDelay receiveDelay = ReceiveDelay.Moderate;
        #endregion
    #region Stop Receiver At Once (Deprecation for game?)
        /// <summary>
        /// Use this from hub not to call some methods in Main-Assembly from SynicSugar.dll. </ br>
        /// Stop packet receeiveing to buffer. Packets are discarded while stopped.
        /// </summary>
        public void PausePacketReceiving(){
            p2pToken.Cancel();
            CloseConnection();
        }
        /// <summary>
        /// Use this from hub not to call some methods in Main-Assembly from SynicSugar.dll. </ br>
        /// Prepare to receive in advance. If user sent packets, it can open to get packets for a socket id without this.
        /// </summary>
        public void ReStartPacketReceiving(){
            p2pToken = new CancellationTokenSource();
            OpenConnection();
        }
    #endregion
        /// <summary>
        /// Use this from hub not to call some methods in Main-Assembly from SynicSugar.dll.</ br>
        /// Stop receiver, close all connects and cancel all events to receive.<br />
        /// To exit the current lobby.
        /// </summary>
        public void EndConnection(){
            p2pToken.Cancel();
            CloseConnection();
            Destroy(this.gameObject);
        }
        /// <summary>
        /// Use this from hub not to call some methods in Main-Assembly from SynicSugar.dll.
        /// </summary>
        public SugarPacket GetPacketFromBuffer(){
            //Set options
            ReceivePacketOptions options = new ReceivePacketOptions(){
                LocalUserId = userIds.LocalUserId.AsEpic,
                MaxDataSizeBytes = 4096,
                RequestedChannel = null
            };
            //Next packet size
            var getNextReceivedPacketSizeOptions = new GetNextReceivedPacketSizeOptions {
                LocalUserId = userIds.LocalUserId.AsEpic,
                RequestedChannel = null
            };

            P2PHandle.GetNextReceivedPacketSize(ref getNextReceivedPacketSizeOptions, out uint nextPacketSizeBytes);

            byte[] data = new byte[nextPacketSizeBytes];
            var dataSegment = new ArraySegment<byte>(data);
            Result result = P2PHandle.ReceivePacket(ref options, out ProductUserId peerId, out SocketId socketId, out byte outChannel, dataSegment, out uint bytesWritten);
            
            if (result != Result.Success){
                return null; //No packet
            }
            return new SugarPacket(){ ch = outChannel, UserID = peerId.ToString(), payload = dataSegment}; 
        }
#region Notify(ConnectRquest)
        // MAYBE: Probably uint to determine if the request notification has been sent out, and since we allow reception for all SocketIDs (=SocketName), there is no need to call it multiple times.
        // uint for performance instead of bool?
        void SubscribeToConnectionRequest(){
            if (RequestNotifyId == 0){
                AddNotifyPeerConnectionRequestOptions options = new AddNotifyPeerConnectionRequestOptions(){
                    LocalUserId = EOSManager.Instance.GetProductUserId(),
                    SocketId = SocketId
                };

                RequestNotifyId = P2PHandle.AddNotifyPeerConnectionRequest(ref options, null, OnIncomingConnectionRequest);
                
                if (RequestNotifyId == 0){
                    Debug.Log("Connection Request: could not subscribe, bad notification id returned.");
                }
            }
        }
        // Call from SubscribeToConnectionRequest.
        // This function will only be called if the connection has not already been accepted.
        void OnIncomingConnectionRequest(ref OnIncomingConnectionRequestInfo data){
            if (!(bool)data.SocketId?.SocketName.Equals(ScoketName)){
                Debug.LogError("ConnectRequest: unknown socket id. This peer should be no lobby member.");
                return;
            }

            AcceptConnectionOptions options = new AcceptConnectionOptions(){
                LocalUserId = EOSManager.Instance.GetProductUserId(),
                RemoteUserId = data.RemoteUserId,
                SocketId = SocketId
            };

            Result result = P2PHandle.AcceptConnection(ref options);

            if (result != Result.Success){
                Debug.LogErrorFormat("p2p connect request: error while accepting connection, code: {0}", result);
            }
        }
        // Stop accepting new connect.
        // ??? Can unsubscribe this notify in game when this player can connect all other pears?
        void UnsubscribeFromConnectionRequest(){
            P2PHandle.RemoveNotifyPeerConnectionRequest(RequestNotifyId);
            RequestNotifyId = 0;
        }
#endregion
#region Connect
    /// <summary>
    /// Prep for p2p connections.
    /// Call from the library after the MatchMake is established.
    /// </summary>
    internal void OpenConnection(){
        SubscribeToConnectionRequest();
    }
#endregion
#region Disconnect
        void CloseConnection (){
            UnsubscribeFromConnectionRequest();
            var closeOptions = new CloseConnectionsOptions(){
                LocalUserId = userIds.LocalUserId.AsEpic,
                SocketId = SocketId
            };
            Result result = P2PHandle.CloseConnections(ref closeOptions);
            if(result != Result.Success){
                Debug.LogErrorFormat("CloseConnections: {0}", result);
            }
        }
#endregion
    }
}