using PlayEveryWare.EpicOnlineServices;
using Epic.OnlineServices;
using Epic.OnlineServices.P2P;
using UnityEngine;
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using SynicSugar.MatchMake;
using SynicSugar.RTC;
using ResultE = Epic.OnlineServices.Result;
//We can't call the main-Assembly from own-assemblies.
//So, use such processes through this assembly.
//TODO: Change it so that this class can only be used from ConnectHub.
namespace SynicSugar.P2P {
    [RequireComponent(typeof(PacketReceiveOnUpdate))]
    [RequireComponent(typeof(PacketReceiveOnLateUpdate))]
    [RequireComponent(typeof(PacketReceiveOnFixedUpdate))]
    public class p2pConnectorForOtherAssembly : MonoBehaviour {
#region Singleton
        public static p2pConnectorForOtherAssembly Instance { get; private set; }
        void Awake() {
            if( Instance != null ) {
                Destroy( this );
                return;
            }
            Instance = this;
            //Packet Receiver
            FixedUpdateReceiver = this.GetComponent<PacketReceiveOnFixedUpdate>();
            UpdateReceiver = this.GetComponent<PacketReceiveOnUpdate>();
            LateUpdateReceiver = this.GetComponent<PacketReceiveOnLateUpdate>();
            
            FixedUpdateReceiver.enabled = false;
            UpdateReceiver.enabled = false;
            LateUpdateReceiver.enabled = false;
        }
        void OnDestroy() {
            if( Instance == this ) {
                Instance = null;
            }
        }
        void Start(){
            SetIntervalSeconds();
            P2PHandle = EOSManager.Instance.GetEOSPlatformInterface().GetP2PInterface();

            //Next packet size
            standardPacketSizeOptions = new GetNextReceivedPacketSizeOptions {
                LocalUserId = EOSManager.Instance.GetProductUserId(),
                RequestedChannel = null
            };

            synicPacketSizeOptions = new GetNextReceivedPacketSizeOptions {
                LocalUserId = EOSManager.Instance.GetProductUserId(),
                RequestedChannel = 255
            };
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

        ulong RequestNotifyId, InterruptedNotify, EstablishedNotify, ClosedNotify;
        public CancellationTokenSource p2pToken;

        //Packet Receiver
        PacketReceiveTiming currentTiming;
        bool isReceiving;
        PacketReceiveOnFixedUpdate FixedUpdateReceiver;
        PacketReceiveOnUpdate UpdateReceiver;
        PacketReceiveOnLateUpdate LateUpdateReceiver;
        /// <summary>
        /// To get packets
        /// </summary>
        GetNextReceivedPacketSizeOptions standardPacketSizeOptions, synicPacketSizeOptions;
        uint nextPacketSizeBytes = 0;
        

    #region Pause Session
        /// <summary>
        /// For ConnectManager. Stop packet receeiveing to buffer. While stopping, packets are dropped.
        /// </summary>
        /// <param name="isForced">If True, stop and clear current packet queue. </ br>
        /// If false, process current queue, then stop it.</param>
        /// <param name="token">For this task</param>
        public async UniTask PauseConnections(bool isForced, CancellationToken token){
            if(isForced){
                ResetConnections();
                return;
            }
            
            CloseConnection();
            
            GetPacketQueueInfoOptions options = new GetPacketQueueInfoOptions();
            PacketQueueInfo info = new PacketQueueInfo();
            P2PHandle.GetPacketQueueInfo(ref options, out info);

            while (info.IncomingPacketQueueCurrentPacketCount > 0){
                await UniTask.Delay(receiverInterval, cancellationToken: token);
                P2PHandle.GetPacketQueueInfo(ref options, out info);
            }

            StopPacketReceiving();
            p2pToken.Cancel();
        }
        /// <summary>
        /// Prepare to receive in advance. If user sent packets, it can open to get packets for a socket id without this.
        /// </summary>
        public void RestartConnections(){
            OpenConnection();
        }
    #endregion
        /// <summary>
        /// Use this from hub not to call some methods in Main-Assembly from SynicSugar.dll.</ br>
        /// Stop connections, exit current lobby.<br />
        /// The Last user closes lobby.
        /// </summary>
        public async UniTask<bool> ExitSession(bool destroyManager, CancellationToken token){
            ResetConnections();
            bool canExit = true;
            //The last user
            if (p2pInfo.Instance.IsHost() && p2pInfo.Instance.AllUserIds.Count == 1){
                canExit = await MatchMakeManager.Instance.CloseCurrentLobby(token);
            }else{
                canExit = await MatchMakeManager.Instance.ExitCurrentLobby(token);
            }
            
            if(destroyManager){
                Destroy(this.gameObject);
            }
            return canExit;
        }
        /// <summary>
        /// Use this from hub not to call some methods in Main-Assembly from SynicSugar.dll.</ br>
        /// Stop connections, exit current lobby.<br />
        /// Host closes lobby. Guest leaves lobby. <br />
        /// If host call this after the lobby has other users, Guests in this lobby are kicked out from the lobby.
        /// </summary>
        public async UniTask<bool> CloseSession(bool destroyManager, CancellationToken token){
            ResetConnections();
            bool canLeave = true;
            if(p2pInfo.Instance.IsHost()){
                canLeave = await MatchMakeManager.Instance.CloseCurrentLobby(token);
            }else{
                canLeave = await MatchMakeManager.Instance.ExitCurrentLobby(token);
            }
            if(destroyManager){
                Destroy(this.gameObject);
            }
            return canLeave;
        }
        /// <summary>
        /// Use this from hub not to call some methods in Main-Assembly from SynicSugar.dll. <br />
        /// TODO: Change it so that this class can only be used from ConnectHub.
        /// </summary>
        public void StartPacketReceiver(IPacketReciver hubInstance, PacketReceiveTiming timing, int maxBatchSize){
            if(isReceiving){
                //Delete in future.
                //Want to avoid using UniTask for packet receive.
                p2pToken.Cancel();
                StopPacketReceiving();
            }

            p2pToken = new CancellationTokenSource();

            if(p2pConfig.Instance.AutoRefreshPing){
                AutoRefreshPings(p2pToken.Token).Forget();
            }
            
            switch(timing){
                case PacketReceiveTiming.FixedUpdate:
                    FixedUpdateReceiver.StartPacketReceiving(hubInstance, maxBatchSize);
                break;
                case PacketReceiveTiming.Update:
                    UpdateReceiver.StartPacketReceiving(hubInstance, maxBatchSize);
                break;
                case PacketReceiveTiming.LateUpdate:
                    LateUpdateReceiver.StartPacketReceiving(hubInstance, maxBatchSize);
                break;
            }
            
            if(IsEnableRTC){
                RTCManager.Instance.ToggleReceiveingFromTarget(null, true);
            }
            currentTiming = timing;
            isReceiving = true;
        }
        public void StopPacketReceiving(){
            if(!isReceiving){
                Debug.Log("StopPacketReceiving: PacketReciver is not working now.");
                return;
            }
            switch(currentTiming){
                case PacketReceiveTiming.FixedUpdate:
                    FixedUpdateReceiver.StopPacketReceiving();
                break;
                case PacketReceiveTiming.Update:
                    UpdateReceiver.StopPacketReceiving();
                break;
                case PacketReceiveTiming.LateUpdate:
                    LateUpdateReceiver.StopPacketReceiving();
                break;
            }
            isReceiving = false;
        }
        /// <summary>
        /// Use this from hub not to call some methods in Main-Assembly from SynicSugar.dll.
        /// </summary>
        public bool GetPacketFromBuffer(ref byte ch, ref string id, ref ArraySegment<byte> payload){
            P2PHandle.GetNextReceivedPacketSize(ref standardPacketSizeOptions, out nextPacketSizeBytes);
            if(nextPacketSizeBytes == 0){
                return false;
            }
            //Set options
            ReceivePacketOptions options = new ReceivePacketOptions(){
                LocalUserId = p2pInfo.Instance.userIds.LocalUserId.AsEpic,
                MaxDataSizeBytes = nextPacketSizeBytes,
                RequestedChannel = null
            };

            byte[] data = new byte[nextPacketSizeBytes];
            payload = new ArraySegment<byte>(data);
            ResultE result = P2PHandle.ReceivePacket(ref options, out ProductUserId peerId, out SocketId socketId, out ch, payload, out uint bytesWritten);
            
            if (result != ResultE.Success){
#if SYNICSUGAR_LOG //This range is for performance since this is called every frame.
                if(result == ResultE.InvalidParameters){
                    Debug.LogErrorFormat("Get Packets: input was invalid: {0}", result);
                }
#endif
                return false; //No packet
            }
            id = UserId.GetUserId(peerId).ToString();
        #if SYNICSUGAR_PACKETINFO
            Debug.Log($"PacketInfo: ch {ch} / user {id} / length {bytesWritten} / payload {EOSp2p.ByteArrayToHexString(data)}");
        #endif

            return true;
        }
        /// <summary>
        /// To get only SynicPacket.
        /// Use this from ConenctHub not to call some methods in Main-Assembly from SynicSugar.dll.
        /// </summary>
        public bool GetSynicPacketFromBuffer(ref byte ch, ref string id, ref ArraySegment<byte> payload){
            P2PHandle.GetNextReceivedPacketSize(ref synicPacketSizeOptions, out nextPacketSizeBytes);
            if(nextPacketSizeBytes == 0){
                return false;
            }
            //Set options
            ReceivePacketOptions options = new ReceivePacketOptions(){
                LocalUserId = p2pInfo.Instance.userIds.LocalUserId.AsEpic,
                MaxDataSizeBytes = nextPacketSizeBytes,
                RequestedChannel = 255
            };

            byte[] data = new byte[nextPacketSizeBytes];
            payload = new ArraySegment<byte>(data);
            ResultE result = P2PHandle.ReceivePacket(ref options, out ProductUserId peerId, out SocketId socketId, out ch, payload, out uint bytesWritten);
            
            if (result != ResultE.Success){
#if SYNICSUGAR_LOG //This range is for performance since this is called every frame.
                if(result == ResultE.InvalidParameters){
                    Debug.LogErrorFormat("Get Synic Packets: input was invalid: {0}", result);
                }
#endif
                return false; //No packet
            }
            id = UserId.GetUserId(peerId).ToString();
        #if SYNICSUGAR_PACKETINFO
            Debug.Log($"SynicPacketInfo: ch {ch} / user {id} / length {bytesWritten} / payload {EOSp2p.ByteArrayToHexString(data)}");
        #endif

            return true;
        }
        /// <summary>
        /// Clear the packet queues.
        /// Just for PausePacketXXX.
        /// </summary>
        void ClearPacketQueue(){
            ClearPacketQueueOptions options = new ClearPacketQueueOptions(){
                LocalUserId = p2pInfo.Instance.userIds.LocalUserId.AsEpic,
                SocketId = SocketId
            };

            foreach(var id in p2pInfo.Instance.userIds.RemoteUserIds){
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
        /// <summary>
        /// Ready to receive packets of users in the same socket.
        /// </summary>
        void AddNotifyPeerConnectionRequest(){
            if (RequestNotifyId == 0){
                AddNotifyPeerConnectionRequestOptions options = new AddNotifyPeerConnectionRequestOptions(){
                    LocalUserId = p2pInfo.Instance.userIds.LocalUserId.AsEpic,
                    SocketId = SocketId
                };

                RequestNotifyId = P2PHandle.AddNotifyPeerConnectionRequest(ref options, null, OnIncomingConnectionRequest);
                
                if (RequestNotifyId == 0){
                    Debug.LogError("Connection Request: could not subscribe, bad notification id returned.");
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
                LocalUserId = p2pInfo.Instance.userIds.LocalUserId.AsEpic,
                RemoteUserId = data.RemoteUserId,
                SocketId = SocketId
            };

            ResultE result = P2PHandle.AcceptConnection(ref options);

            if (result != ResultE.Success){
                Debug.LogErrorFormat("p2p connect request: error while accepting connection, code: {0}", result);
            }
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
    //* Maybe: Some processes in InitConnectConfig need time to complete and the Member list will be created after that end. Therefore, we will add Notify first to spent time.
    internal void OpenConnection(bool checkInitConnect = false){
        AddNotifyPeerConnectionRequest();
        AcceptAllConenctions();

        if(checkInitConnect || p2pConfig.Instance.UseDisconnectedEarlyNotify){
            AddNotifyPeerConnectionEstablished();
        }
        if(p2pConfig.Instance.UseDisconnectedEarlyNotify){
            AddNotifyPeerConnectionInterrupted();
            AddNotifyPeerConnectionClosed();
        }
    }
    //Reason: This order(Receiver, Connection, Que) is that if the RPC includes Rpc to reply, the connections are automatically re-started.
    /// <summary>
    /// Stop packet reciver, clse connections, then clear PacketQueue(incoming and outgoing).
    /// </summary>
    void ResetConnections(){
        StopPacketReceiving();
        p2pToken?.Cancel();
        CloseConnection();
        ClearPacketQueue();
    }
    /// <summary>
    /// For the end of matchmaking. <br />
    /// Immediate packet reception permission in advance
    /// </summary>
    void AcceptAllConenctions(){
        ResultE result = ResultE.Success;
        foreach(var id in p2pInfo.Instance.userIds.RemoteUserIds){
            AcceptConnectionOptions options = new AcceptConnectionOptions(){
                LocalUserId = p2pInfo.Instance.userIds.LocalUserId.AsEpic,
                RemoteUserId = id.AsEpic,
                SocketId = SocketId
            };
            
            result = P2PHandle.AcceptConnection(ref options);

            if (result != ResultE.Success){
                Debug.LogErrorFormat("Accept All Connections: error while accepting connection, code: {0} / id: {1}", result, id);
                break;
            }
        }
    }
#endregion
#region Early Connected Notify
    void AddNotifyPeerConnectionInterrupted(){
        if (InterruptedNotify == 0){
            AddNotifyPeerConnectionInterruptedOptions options = new AddNotifyPeerConnectionInterruptedOptions(){
                LocalUserId = p2pInfo.Instance.userIds.LocalUserId.AsEpic,
                SocketId = SocketId
            };

            InterruptedNotify = P2PHandle.AddNotifyPeerConnectionInterrupted(ref options, null, OnPeerConnectionInterruptedCallback);
            
            if (InterruptedNotify == 0){
                Debug.LogError("Connection Request: could not subscribe, bad notification id returned.");
            }
        }
    }
    // Call from SubscribeToConnectionRequest.
    // This function will only be called if the connection has not been accepted yet.
    void OnPeerConnectionInterruptedCallback(ref OnPeerConnectionInterruptedInfo data){
        if (!(bool)data.SocketId?.SocketName.Equals(ScoketName)){
            Debug.LogError("InterruptedCallback: unknown socket id. This peer should be no lobby member.");
            return;
        }
        //Users with young index send Heartbeat.
        if(p2pInfo.Instance.GetUserIndex(p2pInfo.Instance.LocalUserId) <= 2){
            p2pInfo.Instance.RefreshPing(UserId.GetUserId(data.RemoteUserId)).Forget();
            MatchMakeManager.Instance.UpdateMemberAttributeAsHeartBeat(p2pInfo.Instance.GetUserIndex(UserId.GetUserId(data.RemoteUserId)));
        }

        p2pInfo.Instance.ConnectionNotifier.EarlyDisconnected(UserId.GetUserId(data.RemoteUserId), Reason.Interrupted);
    #if SYNICSUGAR_LOG
        Debug.Log("PeerConnectionInterrupted: Connection lost now. with " +  data.RemoteUserId);
    #endif
    }
    void RemoveNotifyPeerConnectionInterrupted(){
        P2PHandle.RemoveNotifyPeerConnectionInterrupted(InterruptedNotify);
        InterruptedNotify = 0;
    }
    void AddNotifyPeerConnectionEstablished(){
        if (EstablishedNotify == 0){
            AddNotifyPeerConnectionEstablishedOptions options = new AddNotifyPeerConnectionEstablishedOptions(){
                LocalUserId = p2pInfo.Instance.userIds.LocalUserId.AsEpic,
                SocketId = SocketId
            };

            EstablishedNotify = P2PHandle.AddNotifyPeerConnectionEstablished(ref options, null, OnPeerConnectionEstablishedCallback);
            
            if (EstablishedNotify == 0){
                Debug.LogError("Connection Request: could not subscribe, bad notification id returned.");
            }
        }
    }
    // Call from SubscribeToConnectionRequest.
    // This function will only be called if the connection has not been accepted yet.
    void OnPeerConnectionEstablishedCallback(ref OnPeerConnectionEstablishedInfo data){
        if (!(bool)data.SocketId?.SocketName.Equals(ScoketName)){
            Debug.LogError("EstablishedCallback: unknown socket id. This peer should be no lobby member.");
            return;
        }
        if(data.ConnectionType == ConnectionEstablishedType.Reconnection){
            p2pInfo.Instance.ConnectionNotifier.Restored(UserId.GetUserId(data.RemoteUserId));
    #if SYNICSUGAR_LOG
            Debug.Log("EstablishedCallback: Connection is restored.");
    #endif
            return;
        }
        
        if(data.ConnectionType == ConnectionEstablishedType.NewConnection &&
            p2pInfo.Instance.userIds.RemoteUserIds.Contains(UserId.GetUserId(data.RemoteUserId))){
            p2pInfo.Instance.ConnectionNotifier.OnEstablished();
            return;
        }
    }
    internal void RemoveNotifyPeerConnectionnEstablished(){
        P2PHandle.RemoveNotifyPeerConnectionEstablished(EstablishedNotify);
        EstablishedNotify = 0;
    }
    void AddNotifyPeerConnectionClosed(){
        if (ClosedNotify == 0){
            AddNotifyPeerConnectionClosedOptions options = new AddNotifyPeerConnectionClosedOptions(){
                LocalUserId = p2pInfo.Instance.userIds.LocalUserId.AsEpic,
                SocketId = SocketId
            };

            ClosedNotify = P2PHandle.AddNotifyPeerConnectionClosed(ref options, null, OnPeerConnectionClosedCallback);
            
            if (ClosedNotify == 0){
                Debug.LogError("Connection Request: could not subscribe, bad notification id returned.");
            }
        }
    }
    // Call from SubscribeToConnectionRequest.
    // This function will only be called if the connection has not been accepted yet.
    void OnPeerConnectionClosedCallback(ref OnRemoteConnectionClosedInfo data){
        if (!(bool)data.SocketId?.SocketName.Equals(ScoketName)){
            Debug.LogError("ClosedCallback: unknown socket id. This peer should be no lobby member.");
            return;
        }
        //Users with young index send Heartbeat.
        if(p2pInfo.Instance.GetUserIndex(p2pInfo.Instance.LocalUserId) <= 2){
            //+100 is second's symbol.
            int disconnectedUserIndex = 100 + p2pInfo.Instance.GetUserIndex(UserId.GetUserId(data.RemoteUserId));
            MatchMakeManager.Instance.UpdateMemberAttributeAsHeartBeat(disconnectedUserIndex);
        }
    #if SYNICSUGAR_LOG
        Debug.Log("PeerConnectionClosedCallback: Connection lost now. with " +  data.RemoteUserId);
    #endif
    }
    internal void RemoveNotifyPeerConnectionnClosed(){
        P2PHandle.RemoveNotifyPeerConnectionClosed(ClosedNotify);
        ClosedNotify = 0;
    }
#endregion
#region Disconnect
        void CloseConnection (){
            RemoveNotifyPeerConnectionRequest();
            if(p2pConfig.Instance.UseDisconnectedEarlyNotify){
                RemoveNotifyPeerConnectionInterrupted();
                RemoveNotifyPeerConnectionnEstablished();
                RemoveNotifyPeerConnectionnClosed();
            }

            var closeOptions = new CloseConnectionsOptions(){
                LocalUserId = p2pInfo.Instance.userIds.LocalUserId.AsEpic,
                SocketId = SocketId
            };
            ResultE result = P2PHandle.CloseConnections(ref closeOptions);
            if(result != ResultE.Success){
                Debug.LogErrorFormat("CloseConnections: Failed to disconnect {0}", result);
            }
        }
#endregion
        /// <summary>
        /// Update SyncedInfo, then Invoke SyncedSynic event.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="phase"></param>
        /// <summary>
        public void UpdateSyncedState(string id, byte phase){
            p2pInfo.Instance.SyncSnyicNotifier.UpdateSyncedState(id, phase);
        }
        public async UniTask AutoRefreshPings(CancellationToken token){
            await UniTask.Delay(p2pConfig.Instance.PingAutoRefreshRateSec * 1000);
            if(token.IsCancellationRequested){ return; }

            await p2pInfo.Instance.pings.RefreshPings(token);
            if(token.IsCancellationRequested){ return; }
            
            AutoRefreshPings(token).Forget();
        }
        /// <summary>
        /// Change AcceptHostsSynic to false. Call from ConnectHub
        /// </summary>
        public void CloseHostSynic(){
            p2pInfo.Instance.userIds.ReceivedocalUserSynic();
        }
        public void GetPong(string id, ArraySegment<byte> utc){
            p2pInfo.Instance.pings.GetPong(id, utc);
        }
        public bool IsEnableRTC => MatchMakeManager.Instance.eosLobby.CurrentLobby.hasConnectedRTCRoom;

    #region Obsolete
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

    #endregion
    }
}