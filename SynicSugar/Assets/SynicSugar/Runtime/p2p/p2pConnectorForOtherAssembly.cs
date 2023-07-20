using PlayEveryWare.EpicOnlineServices;
using Epic.OnlineServices;
using Epic.OnlineServices.P2P;
using UnityEngine;
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using SynicSugar.MatchMake;
using ResultE = Epic.OnlineServices.Result;
//We can't call the main-Assembly from own-assemblies.
//So, use such processes through this assembly.
namespace SynicSugar.P2P {
    public class p2pConnectorForOtherAssembly : MonoBehaviour {
#region Singleton
        private p2pConnectorForOtherAssembly(){}
        public static p2pConnectorForOtherAssembly Instance { get; private set; }
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
        void Start(){
            SetIntervalSeconds();
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
        public int receiverInterval { get; private set; } = 20;
        void SetIntervalSeconds(){
            switch(p2pConfig.Instance.getPacketFrequency){
                case p2pConfig.GetPacketFrequency.PerSecondFPS:
                receiverInterval = 0;
                break;
                case p2pConfig.GetPacketFrequency.PerSecond100:
                receiverInterval = 10;
                break;
                case p2pConfig.GetPacketFrequency.PerSecond50:
                receiverInterval = 20;
                break;
                case p2pConfig.GetPacketFrequency.PerSecond25:
                receiverInterval = 40;
                break;
            }
        }

    #region Pause Session(Experimental, Not recommend for game?)
        /// <summary>
        /// For ConnectManager. Stop packet receeiveing to buffer. While stopping, packets are dropped.
        /// </summary>
        /// <param name="isForced">If True, stop and clear current packet queue. </ br>
        /// If false, process current queue, then stop it.</param>
        /// <param name="token">For this task</param>
        public async UniTask PauseConnections(bool isForced, CancellationTokenSource cancelToken){
            if(isForced){
                ResetConnections();
                return;
            }
            
            CloseConnection();
            if(cancelToken == default(CancellationTokenSource)){
                cancelToken = new CancellationTokenSource();
            }
            
            GetPacketQueueInfoOptions options = new GetPacketQueueInfoOptions();
            PacketQueueInfo info = new PacketQueueInfo();

            while (info.IncomingPacketQueueCurrentPacketCount >= 0){
                P2PHandle.GetPacketQueueInfo(ref options, out info);
                await UniTask.Delay(receiverInterval, cancellationToken: cancelToken.Token);
            }

            p2pToken.Cancel();
        }
        /// <summary>
        /// Prepare to receive in advance. If user sent packets, it can open to get packets for a socket id without this.
        /// </summary>
        public void RestartConnections(){
            OpenConnection();

            p2pToken = new CancellationTokenSource();
        }
    #endregion
        /// <summary>
        /// Use this from hub not to call some methods in Main-Assembly from SynicSugar.dll.</ br>
        /// Stop connections, exit current lobby.<br />
        /// To just leave from the current lobby.(This is not destroying it)
        /// </summary>
        public async UniTask<bool> ExitSession(CancellationToken token){
            ResetConnections();

            bool canExit = await MatchMakeManager.Instance.ExitCurrentLobby(token);
            
            Destroy(this.gameObject);
            return canExit;
        }
        /// <summary>
        /// Use this from hub not to call some methods in Main-Assembly from SynicSugar.dll.</ br>
        /// Stop connections, exit current lobby.<br />
        /// To just leave from the current lobby.(This is not destroying it)
        /// </summary>
        public async UniTask<bool> CloseSession(CancellationToken token){
            ResetConnections();
            bool canLeave = true;
            if(p2pConfig.Instance.userIds.IsHost()){
                canLeave = await MatchMakeManager.Instance.CloseCurrentLobby(token);
            }else{
                canLeave = await MatchMakeManager.Instance.ExitCurrentLobby(token);
            }
            Destroy(this.gameObject);
            return canLeave;
        }

        /// <summary>
        /// Use this from hub not to call some methods in Main-Assembly from SynicSugar.dll.
        /// </summary>
        public SugarPacket GetPacketFromBuffer(){
            //Set options
            ReceivePacketOptions options = new ReceivePacketOptions(){
                LocalUserId = p2pConfig.Instance.userIds.LocalUserId.AsEpic,
                MaxDataSizeBytes = 1170,
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
            ResultE result = P2PHandle.ReceivePacket(ref options, out ProductUserId peerId, out SocketId socketId, out byte outChannel, dataSegment, out uint bytesWritten);
            
            if (result != ResultE.Success){
#if SYNICSUGAR_LOG //This range is for performance since this is called every frame.
                if(result == ResultE.InvalidParameters){
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

                ResultE result = P2PHandle.ClearPacketQueue(ref options);
                
                if (result != ResultE.Success){
                    Debug.LogErrorFormat("Clear Queue: can't clear packet queue, code: {0}", result);
                    return;
                }
            }
        #if SYNICSUGAR_LOG
            Debug.Log("Clear Queue: Finish!");
        #endif
        }
#region Notify(ConnectRquest)
        void AddNotifyPeerConnectionRequest(){
            if (RequestNotifyId == 0){
                AddNotifyPeerConnectionRequestOptions options = new AddNotifyPeerConnectionRequestOptions(){
                    LocalUserId = p2pConfig.Instance.userIds.LocalUserId.AsEpic,
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
                LocalUserId = p2pConfig.Instance.userIds.LocalUserId.AsEpic,
                RemoteUserId = data.RemoteUserId,
                SocketId = SocketId
            };

            ResultE result = P2PHandle.AcceptConnection(ref options);

            if (result != ResultE.Success){
                Debug.LogErrorFormat("p2p connect request: error while accepting connection, code: {0}", result);
            }
        #if SYNICSUGAR_LOG
            Debug.Log("p2p connect request: Success Connect Request");
        #endif
        }
        void RemoveNotifyPeerConnectionRequest(){
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
        AddNotifyPeerConnectionRequest();
        AcceptAllConenctions();
    }
    //Reason: This order(Receiver, Connection, Que) is that if the RPC includes Rpc to reply, the connections are automatically re-started.
    /// <summary>
    /// Stop packet reciver, clse connections, then clear PacketQueue(incoming and outgoing).
    /// </summary>
    void ResetConnections(){
        p2pToken.Cancel();
        CloseConnection();
        ClearPacketQueue();
    }
    /// <summary>
    /// For the end of matchmaking.
    /// </summary>
    void AcceptAllConenctions(){
        AcceptConnectionOptions options = new AcceptConnectionOptions(){
                LocalUserId = p2pConfig.Instance.userIds.LocalUserId.AsEpic,
                SocketId = SocketId
            };
        ResultE result = ResultE.Success;
        foreach(var id in p2pConfig.Instance.userIds.RemoteUserIds){
            options.RemoteUserId = id.AsEpic;
            result = P2PHandle.AcceptConnection(ref options);

            if (result != ResultE.Success){
                Debug.LogErrorFormat("Re-accept All Connections: error while accepting connection, code: {0} / id: {1}", result, id);
                break;
            }
        }
        if(result != ResultE.Success){
            return;
        }
        #if SYNICSUGAR_LOG
            Debug.Log("Re-accept All Connections: Success accept Connections");
        #endif
    }
#endregion
#region Disconnect
        void CloseConnection (){
            RemoveNotifyPeerConnectionRequest();

            var closeOptions = new CloseConnectionsOptions(){
                LocalUserId = p2pConfig.Instance.userIds.LocalUserId.AsEpic,
                SocketId = SocketId
            };
            ResultE result = P2PHandle.CloseConnections(ref closeOptions);
            Debug.Log($"close result is {result} / {ScoketName}");
            if(result != ResultE.Success){
                Debug.LogErrorFormat("CloseConnections: Failed to disconnect {0}", result);
            }
        }
#endregion
    }
}