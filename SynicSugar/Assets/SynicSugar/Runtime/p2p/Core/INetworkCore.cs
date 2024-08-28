using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace SynicSugar.P2P {
    public interface INetworkCore {
        /// <summary>
        /// For ConnectManager. Stop packet receeiveing to buffer. While stopping, packets are dropped.
        /// </summary>
        /// <param name="isForced">If True, stop and clear current packet queue. <br />
        /// If false, process current queue, then stop it.</param>
        /// <param name="token">For this task</param>
        UniTask<Result> PauseConnections(bool isForced, CancellationToken token);

        /// <summary>
        /// (Re)open connection.
        /// </summary>
        Result RestartConnections();

        /// <summary>
        /// Use this from hub not to call some methods in Main-Assembly from SynicSugar.dll.<br />
        /// Stop connections, exit current lobby.<br />
        /// The Last user closes lobby.
        /// </summary>
        /// <param name="destroyManager">If true, destroy NetworkManager after cancel matchmake.</param>
        /// <param name="cleanupMemberCountChanged">Need to call MatchMakeManager.Instance.MatchMakingGUIEvents.LobbyMemberCountChanged(id, false) after exit lobby?</param>
        /// <param name="token">token for this task</param>
        UniTask<Result> ExitSession(bool destroyManager, bool cleanupMemberCountChanged, CancellationToken token);

        /// <summary>
        /// Use this from hub not to call some methods in Main-Assembly from SynicSugar.dll.<br />
        /// Stop connections, exit current lobby.<br />
        /// Host closes lobby. Guest leaves lobby. <br />
        /// If host call this after the lobby has other users, Guests in this lobby are kicked out from the lobby.
        /// </summary>
        /// <param name="destroyManager">If true, destroy NetworkManager after cancel matchmake.</param>
        /// <param name="cleanupMemberCountChanged">Need to call MatchMakeManager.Instance.MatchMakingGUIEvents.LobbyMemberCountChanged(id, false) after exit lobby?</param>
        /// <param name="token">token for this task</param>
        UniTask<Result> CloseSession(bool destroyManager, bool cleanupMemberCountChanged, CancellationToken token);

        /// <summary>
        /// Start standart packet receiver on each timing. Only one can be enabled, including Synic.<br />
        /// Use this from hub not to call some methods in Main-Assembly from SynicSugar.dll. 
        /// </summary>
        /// <param name="hubInstance">ConnectHub instance</param>
        /// <param name="timing">The timing that packet receiver gets packet from buffer.</param>
        /// <param name="maxBatchSize">How many times during 1 FPS are received.</param>
        Result StartPacketReceiver(IPacketConvert hubInstance, PacketReceiveTiming timing, uint maxBatchSize);

        /// <summary>
        /// To get only SynicPacket in update in burst FPS. Call after creating the Network Instance required for reception.<br />
        /// This cannot be called with other Receiver same time. If start the other Receiver, ConenctHub stop this Receiver automatically before start the new one.
        /// </summary>
        /// <param name="hubInstance">ConnectHub instance</param>
        /// <param name="maxBatchSize">How many times during 1 FPS are received</param>
        Result StartSynicReceiver(IPacketConvert hubInstance, uint maxBatchSize);

        /// <summary>
        /// Pause getting a packet from the buffer. To re-start, call StartPacketReceiver().<br />
        /// *Packet receiving to the buffer is continue. If the packet is over the buffer, subsequent packets are discarded.
        /// </summary>
        Result StopPacketReceiver();

        /// <summary>
        /// Update SyncedInfo, then Invoke SyncedSynic event.
        /// </summary>
        /// <param name="id">target id</param>
        /// <param name="phase">Synic phase</param>
        /// <summary>
        void UpdateSyncedState(string id, byte phase);

        /// <summary>
        /// Change AcceptHostsSynic to false. Call from ConnectHub
        /// </summary>
        void CloseHostSynic();
        
        /// <summary>
        /// Return pong to calculate RTT. Call from ConnectHub
        /// </summary>
        void GetPong(string id, ArraySegment<byte> utc);
    }
}