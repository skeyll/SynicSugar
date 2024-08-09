using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace SynicSugar.P2P {
    public interface INetworkCore {
        UniTask PauseConnections(bool isForced, CancellationToken token);
        void RestartConnections();
        UniTask<Result> ExitSession(bool destroyManager, CancellationToken token);
        UniTask<Result> CloseSession(bool destroyManager, CancellationToken token);
        void StartPacketReceiver(IPacketConvert hubInstance, PacketReceiveTiming timing, uint maxBatchSize);
        void StartSynicReceiver(IPacketConvert hubInstance, uint maxBatchSize);
        void StopPacketReceiver();
        void UpdateSyncedState(string id, byte phase);
        void CloseHostSynic();
        void GetPong(string id, ArraySegment<byte> utc);
    }
}