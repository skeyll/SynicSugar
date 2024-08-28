using PlayEveryWare.EpicOnlineServices;
using Epic.OnlineServices;
using Epic.OnlineServices.P2P;
using UnityEngine;
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using SynicSugar.MatchMake;
using SynicSugar.Base;
using ResultE = Epic.OnlineServices.Result;
//We can't call the main-Assembly from own-assemblies.
//So, use such processes through this assembly.
namespace SynicSugar.P2P {
    public sealed class EOSSessionManager : SessionCore {
        public EOSSessionManager() : base (){
            P2PHandle = EOSManager.Instance.GetEOSPlatformInterface().GetP2PInterface();

            // To get Next packet size
            standardPacketSizeOptions = new GetNextReceivedPacketSizeOptions {
                LocalUserId = EOSManager.Instance.GetProductUserId(),
                RequestedChannel = null
            };

            synicPacketSizeOptions = new GetNextReceivedPacketSizeOptions {
                LocalUserId = EOSManager.Instance.GetProductUserId(),
                RequestedChannel = 255
            };
        }

        /// <summary>
        /// For catch
        /// </summary>
        internal P2PInterface P2PHandle;
        /// <summary>
        /// For pointer to pass receive packet
        /// </summary>
        public SocketId ReferenceSocketId;

        ulong RequestNotifyId, InterruptedNotify, EstablishedNotify, ClosedNotify;

        /// <summary>
        /// To get packets
        /// </summary>
        GetNextReceivedPacketSizeOptions standardPacketSizeOptions, synicPacketSizeOptions;

        
    #region INetworkCore
        /// <summary>
        /// For ConnectManager. Stop packet receeiveing to buffer. While stopping, packets are dropped.
        /// </summary>
        /// <param name="isForced">If True, stop and clear current packet queue. <br />
        /// If false, process current queue, then stop it.</param>
        /// <param name="token">For this task</param>
        protected override async UniTask<Result> PauseConnections(bool isForced, CancellationToken token){
            if(isForced){
                ResetConnections();
                return Result.Success;
            }
            
            CloseConnection();
            
            GetPacketQueueInfoOptions options = new GetPacketQueueInfoOptions();
            PacketQueueInfo info = new PacketQueueInfo();
            P2PHandle.GetPacketQueueInfo(ref options, out info);

            while (info.IncomingPacketQueueCurrentPacketCount > 0){
                await UniTask.Yield(PlayerLoopTiming.EarlyUpdate, cancellationToken: token);
                P2PHandle.GetPacketQueueInfo(ref options, out info);
            }

            ((INetworkCore)this).StopPacketReceiver();
            return Result.Success;
        }
        /// <summary>
        /// Prepare to receive in advance. If user sent packets, it can open to get packets for a socket id without this.
        /// </summary>
        protected override Result RestartConnections(){
            return InitiateConnection();
        }
        /// <summary>
        /// Use this from hub not to call some methods in Main-Assembly from SynicSugar.dll.<br />
        /// Stop connections, exit current lobby.<br />
        /// The Last user closes lobby.
        /// </summary>
        /// <param name="destroyManager">If true, destroy NetworkManager after cancel matchmake.</param>
        /// <param name="cleanupMemberCountChanged">Need to call MatchMakeManager.Instance.MatchMakingGUIEvents.LobbyMemberCountChanged(id, false) after exit lobby?</param>
        /// <param name="token">token for this task</param>
        protected override async UniTask<Result> ExitSession(bool destroyManager, bool cleanupMemberCountChanged , CancellationToken token){
            ResetConnections();
            Result  exitResult;
            //The last user
            if (p2pInfo.Instance.IsHost() && p2pInfo.Instance.CurrentConnectedUserIds.Count == 1){
                exitResult = await MatchMakeManager.Instance.CloseCurrentLobby(cleanupMemberCountChanged, token);
            }else{
                exitResult = await MatchMakeManager.Instance.ExitCurrentLobby(cleanupMemberCountChanged, token);
            }
            
            if(exitResult != Result.Success){
                return exitResult;
            }
            if(destroyManager){
                Destroy(MatchMakeManager.Instance.gameObject);
            }

            SynicSugarManger.Instance.State.IsInSession = false;
            return Result.Success;
        }
        /// <summary>
        /// Use this from hub not to call some methods in Main-Assembly from SynicSugar.dll.<br />
        /// Stop connections, exit current lobby.<br />
        /// Host closes lobby. Guest leaves lobby. <br />
        /// If host call this after the lobby has other users, Guests in this lobby are kicked out from the lobby.
        /// </summary>
        /// <param name="destroyManager">If true, destroy NetworkManager after cancel matchmake.</param>
        /// <param name="cleanupMemberCountChanged">Need to call MatchMakeManager.Instance.MatchMakingGUIEvents.LobbyMemberCountChanged(id, false) after exit lobby?</param>
        /// <param name="token">token for this task</param>
        protected override async UniTask<Result> CloseSession(bool destroyManager, bool cleanupMemberCountChanged, CancellationToken token){
            ResetConnections();
            Result closeResult;
            if(p2pInfo.Instance.IsHost()){
                closeResult = await MatchMakeManager.Instance.CloseCurrentLobby(cleanupMemberCountChanged, token);
            }else{
                closeResult = await MatchMakeManager.Instance.ExitCurrentLobby(cleanupMemberCountChanged, token);
            }
            if(closeResult != Result.Success){
                return closeResult;
            }
            if(destroyManager){
                Destroy(MatchMakeManager.Instance.gameObject);
            }

            SynicSugarManger.Instance.State.IsInSession = false;
            return Result.Success;
        }
        
        /// <summary>
        /// To get Packetｓ.
        /// Use this from hub not to call some methods in Main-Assembly from SynicSugar.dll.
        /// </summary>
        public override bool GetPacketFromBuffer(ref byte ch, ref ProductUserId id, ref ArraySegment<byte> payload){
            ResultE existPacket = P2PHandle.GetNextReceivedPacketSize(ref standardPacketSizeOptions, out uint nextPacketSizeBytes);
            if(existPacket != ResultE.Success){
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
            ResultE result = P2PHandle.ReceivePacket(ref options, ref id, ref ReferenceSocketId, out ch, payload, out uint bytesWritten);
            
            if (result != ResultE.Success){
#if SYNICSUGAR_LOG //This range is for performance since this is called every frame.
                if(result == ResultE.InvalidParameters){
                    Debug.LogErrorFormat("Get Packets: input was invalid: {0}", result);
                }
#endif
                return false; //No packet
            }
        #if SYNICSUGAR_PACKETINFO
            Debug.Log($"ReceivePacket: {ch.ToString()}({ch}) from {id} / packet size {bytesWritten} / payload {EOSp2p.ByteArrayToHexString(data)}");
        #endif

            return true;
        }
        /// <summary>
        /// To get only SynicPacket.
        /// Use this from ConenctHub not to call some methods in Main-Assembly from SynicSugar.dll.
        /// </summary>
        public override bool GetSynicPacketFromBuffer(ref byte ch, ref ProductUserId id, ref ArraySegment<byte> payload){
            ResultE existPacket = P2PHandle.GetNextReceivedPacketSize(ref synicPacketSizeOptions, out uint nextPacketSizeBytes);
            if(existPacket != ResultE.Success){
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
            ResultE result = P2PHandle.ReceivePacket(ref options, ref id, ref ReferenceSocketId, out ch, payload, out uint bytesWritten);
            
            if (result != ResultE.Success){
#if SYNICSUGAR_LOG //This range is for performance since this is called every frame.
                if(result == ResultE.InvalidParameters){
                    Debug.LogErrorFormat("Get Synic Packets: input was invalid: {0}", result);
                }
#endif
                return false; //No packet
            }
        #if SYNICSUGAR_PACKETINFO
            Debug.Log($"ReceivePacket(Synic): {ch.ToString()}({ch}) from {id} / packet size {bytesWritten} / payload {EOSp2p.ByteArrayToHexString(data)}");
        #endif

            return true;
        }
    
    #endregion
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
    protected override Result InitiateConnection(bool checkInitConnect = false){
        AddNotifyPeerConnectionRequest();
        AcceptAllConenctions();

        if(checkInitConnect || p2pConfig.Instance.UseDisconnectedEarlyNotify){
            AddNotifyPeerConnectionEstablished();
        }
        if(p2pConfig.Instance.UseDisconnectedEarlyNotify){
            AddNotifyPeerConnectionInterrupted();
            AddNotifyPeerConnectionClosed();
        }
        return Result.Success;
    }
    
    //Reason: This order(Receiver, Connection, Que) is that if the RPC includes Rpc to reply, the connections are automatically re-started.
    /// <summary>
    /// Stop packet reciver, clse connections, then clear PacketQueue(incoming and outgoing).
    /// </summary>
    void ResetConnections(){
        ((INetworkCore)this).StopPacketReceiver();
        CancelRTTToken();
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
        protected override Result CloseConnection (){
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
            Result result = (Result)P2PHandle.CloseConnections(ref closeOptions);
            if(result != Result.Success){
                Debug.LogErrorFormat("CloseConnections: Failed to disconnect {0}", result);
            }
            return result;
        }
#endregion
    }
}