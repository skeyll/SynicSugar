using PlayEveryWare.EpicOnlineServices;
using Epic.OnlineServices;
using Epic.OnlineServices.P2P;
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
//We can't call the main-Assembly from own-assemblies.
//So, use such processes through  this assembly .
namespace SynicSugar.P2P {
    public class p2pConnectorForOtherAssembly : MonoBehaviour {
#region Singleton
        private p2pConnectorForOtherAssembly(){}
        public static p2pConnectorForOtherAssembly Instance { get; private set; }
        void Awake() {
            if( Instance != null ) {
                Destroy( this.gameObject );
                return;
            }
            Instance = this;
        }
        void OnDestroy() {
            if( Instance == this ) {
                Instance = null;
            }
        }
        void Start(){
            SetIntervalSeconds(p2pConfig.Instance.receiveInterval);
            P2PHandle = EOSManager.Instance.GetEOSPlatformInterface().GetP2PInterface();
        }
#endregion
        internal P2PInterface P2PHandle;
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
        
        /// <summary>
        /// For internal process Use this 
        /// </summary>
        /// <value></value>
        public int receiverInterval { get; private set; } = 25;
        void SetIntervalSeconds(p2pConfig.ReceiveInterval size){
            if(size == p2pConfig.ReceiveInterval.Large){
                receiverInterval = 50;
            }else if(size == p2pConfig.ReceiveInterval.Moderate){
                receiverInterval = 25;
            }else{
                receiverInterval = 10;
            }
        }

    #region Pause Session(Experimental, Not recommend for game?)
        /// <summary>
        /// Stop packet receeiveing to buffer. Packets are discarded while stopped.
        /// </summary>
        /// <param name="isForced">If True, clear current packet queue. </ br>
        /// If false, process current queue, then stop it.</param>
        public async UniTask PauseSession(bool isForced, CancellationTokenSource cancelToken = default(CancellationTokenSource)){
            CloseConnection();

            if(!isForced){
                GetPacketQueueInfoOptions options = new GetPacketQueueInfoOptions();
                PacketQueueInfo info = new PacketQueueInfo();

                while (info.IncomingPacketQueueCurrentPacketCount <= 0){
                    P2PHandle.GetPacketQueueInfo(ref options, out info);
                    await UniTask.Delay(receiverInterval, cancellationToken: cancelToken.Token);
                }
            }else{
                ClearPacketQueue();
            }

            p2pToken.Cancel();
        }
        /// <summary>
        /// Prepare to receive in advance. If user sent packets, it can open to get packets for a socket id without this.
        /// </summary>
        public void ReStartSession(){
            //MAYBE: This request work only for a new connection request, so, for former peers, we need to accept these by ourself.
            SubscribeToConnectionRequest();
            ReAcceptAllConenctions();

            p2pToken = new CancellationTokenSource();
        }
    #endregion
        /// <summary>
        /// Use this from hub not to call some methods in Main-Assembly from SynicSugar.dll.</ br>
        /// Stop receiver, close all connects and cancel all events to receive.<br />
        /// To exit the current lobby.
        /// </summary>
        public void LeaveSession(){
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
                LocalUserId = p2pConfig.Instance.userIds.LocalUserId.AsEpic,
                MaxDataSizeBytes = 4096,
                RequestedChannel = null
            };
            //Next packet size
            var getNextReceivedPacketSizeOptions = new GetNextReceivedPacketSizeOptions {
                LocalUserId = p2pConfig.Instance.userIds.LocalUserId.AsEpic,
                RequestedChannel = null
            };

            P2PHandle.GetNextReceivedPacketSize(ref getNextReceivedPacketSizeOptions, out uint nextPacketSizeBytes);

            byte[] data = new byte[nextPacketSizeBytes];
            var dataSegment = new ArraySegment<byte>(data);
            Result result = P2PHandle.ReceivePacket(ref options, out ProductUserId peerId, out SocketId socketId, out byte outChannel, dataSegment, out uint bytesWritten);
            
            if (result != Result.Success){
#if SYNICSUGAR_LOG //This range is for performance since this is called every frame.
                if(result == Result.InvalidParameters){
                    Debug.LogErrorFormat("Get Packets: input was invalid: {0}", result);
                }
#endif
                return null; //No packet
            }
            return new SugarPacket(){ ch = outChannel, UserID = peerId.ToString(), payload = dataSegment}; 
        }
        /// <summary>
        /// Clear the packet queues.
        /// Just for PausePacketXXX.
        /// </summary>
        void ClearPacketQueue(){
            ClearPacketQueueOptions options = new ClearPacketQueueOptions(){
                LocalUserId = p2pConfig.Instance.userIds.LocalUserId.AsEpic,
                SocketId = SocketId
            };

            foreach(var id in p2pConfig.Instance.userIds.RemoteUserIds){
                options.RemoteUserId = id.AsEpic;

                Result result = P2PHandle.ClearPacketQueue(ref options);
                
                if (result != Result.Success){
                    Debug.LogErrorFormat("Clear Queue: can't clear packet queue, code: {0}", result);
                    return;
                }
            }
            
            Debug.Log("Clear Queue");
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
        // This function will only be called if the connection has not been accepted yet.
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
        #if SYNICSUGAR_LOG
            Debug.Log("p2p connect request: Success Connect Request");
        #endif
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
    void ReAcceptAllConenctions(){
        AcceptConnectionOptions options = new AcceptConnectionOptions(){
                LocalUserId = EOSManager.Instance.GetProductUserId(),
                SocketId = SocketId
            };
        Result result = Result.Success;
        foreach(var id in p2pConfig.Instance.userIds.RemoteUserIds){
            options.RemoteUserId = id.AsEpic;
            result = P2PHandle.AcceptConnection(ref options);

            if (result != Result.Success){
                Debug.LogErrorFormat("Re-accept All Connections: error while accepting connection, code: {0} / id: {1}", result, id);
                break;
            }
        }
        if(result != Result.Success){
            return;
        }
        #if SYNICSUGAR_LOG
            Debug.Log("Re-accept All Connections: Success accept Connections");
        #endif
    }
#endregion
#region Disconnect
        void CloseConnection (){
            UnsubscribeFromConnectionRequest();
            var closeOptions = new CloseConnectionsOptions(){
                LocalUserId = p2pConfig.Instance.userIds.LocalUserId.AsEpic,
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