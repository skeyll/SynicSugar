using Cysharp.Threading.Tasks;
using UnityEngine;
using System;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using MemoryPack;
namespace SynicSugar.P2P {
    public class p2pPing : MonoBehaviour{
        internal Dictionary<string, PingInformation> pingInfo = new Dictionary<string, PingInformation>();
        int refreshMembers;
        bool isRefreshing;
        /// <summary>
        /// Send 0 + Utc. Measure ping at the time of return 1 + UTC.
        /// </summary> 
        // MEMO: Replace SendPacketToAll when it can be made more efficient.
        internal async UniTask<bool> RefreshPings(CancellationToken token){
            if(isRefreshing){
                return false;
            }
            isRefreshing = true;
            foreach(var id in p2pInfo.Instance.userIds.RemoteUserIds){
                pingInfo.Add(id.ToString(), new PingInformation());
            }
            for(int i = 0; i < p2pConfig.Instance.SamplesPerPing; i++){
                foreach(var id in p2pInfo.Instance.userIds.RemoteUserIds){
                    DateTime utc = DateTime.UtcNow;
                    byte[] utc_b = MemoryPackSerializer.Serialize(utc);

                    EOSp2p.SendPacket(254, utc_b, id);
                }
                await UniTask.Yield();
            }
            await UniTask.WhenAny(UniTask.WaitUntil(() => refreshMembers == p2pInfo.Instance.userIds.RemoteUserIds.Count, cancellationToken: token),
            UniTask.Delay(10000, cancellationToken: token));

            isRefreshing = false;
            return true;
        }
        internal void GetPong(string id, ArraySegment<byte> utc){//byte[] utc){
            DateTime current = DateTime.UtcNow;
            TimeSpan delta = current - MemoryPackSerializer.Deserialize<DateTime>(utc);
            
            pingInfo[id].tmpPings.Add(delta.TotalMilliseconds);

            if(pingInfo[id].tmpPings.Count == p2pConfig.Instance.SamplesPerPing){
                pingInfo[id].Ping = (int)(pingInfo[id].tmpPings.Sum() / p2pConfig.Instance.SamplesPerPing);
                refreshMembers++;
            }
        }
    }
}