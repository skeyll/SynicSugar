using Cysharp.Threading.Tasks;
using System;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using MemoryPack;
namespace SynicSugar.P2P {
    public class p2pPing {
        internal Dictionary<string, PingInformation> pingInfo = new Dictionary<string, PingInformation>();
        int refreshMembers;
        bool isRefreshing;
        enum CHANNELLIST{
            ObtainPing = 253, ReturnPong = 254, Synic = 255
        }
        /// <summary>
        /// Send 0 + Utc. Measure ping at the time of return 1 + UTC.
        /// </summary> 
        // MEMO: Replace SendPacketToAll when it can be made more efficient.
        internal async UniTask<bool> RefreshPings(CancellationToken token){
            if(isRefreshing){
                return false;
            }
            isRefreshing = true;
            refreshMembers = 0;
            foreach(var id in p2pInfo.Instance.userIds.RemoteUserIds){
                if(!pingInfo.ContainsKey(id.ToString())){
                    pingInfo.Add(id.ToString(), new PingInformation());
                }
            }
            for(int i = 0; i < p2pConfig.Instance.SamplesPerPing; i++){
                foreach(var id in p2pInfo.Instance.userIds.RemoteUserIds){
                    DateTime utc = DateTime.UtcNow;
                    byte[] utc_b = MemoryPackSerializer.Serialize(utc);

                    EOSp2p.SendPacket((byte)CHANNELLIST.ObtainPing, utc_b, id);
                }
                await UniTask.Yield();
            }
            await UniTask.WhenAny(UniTask.WaitUntil(() => refreshMembers == p2pInfo.Instance.userIds.RemoteUserIds.Count, cancellationToken: token),
            UniTask.Delay(10000, cancellationToken: token));

            isRefreshing = false;
            return true;
        }
        //Get Pong and calc
        internal void GetPong(string id, ArraySegment<byte> utc){
            DateTime current = DateTime.UtcNow;
            UnityEngine.Debug.Log("GetPong");
            //For init
            if(!isRefreshing && pingInfo[id].Ping != 0){
                return;
            }

            TimeSpan delta = current - MemoryPackSerializer.Deserialize<DateTime>(utc);
            pingInfo[id].tmpPings.Add(delta.TotalMilliseconds);

            if(pingInfo[id].tmpPings.Count == p2pConfig.Instance.SamplesPerPing){
                pingInfo[id].Ping = (int)(pingInfo[id].tmpPings.Sum() / pingInfo[id].tmpPings.Count);
                pingInfo[id].LastUpdatedLocalUTC = current;
                pingInfo[id].tmpPings.Clear();
                refreshMembers++;
            }
        }
        /// <summary>
        /// For MatchMaking. Send 0 + Utc. Measure ping at the time of return 1 + UTC. Sample is 3
        /// </summary> 
        // MEMO: Replace SendPacketToAll when it can be made more efficient.
        internal async UniTask<bool> InitPings(CancellationToken token){
            foreach(var id in p2pInfo.Instance.userIds.RemoteUserIds){
                if(!pingInfo.ContainsKey(id.ToString())){
                    pingInfo.Add(id.ToString(), new PingInformation());
                }
            }
            RecivePongsPacket(token).Forget();

            for(int i = 0; i < 10; i++){
                foreach(var id in p2pInfo.Instance.userIds.RemoteUserIds){
                    if(pingInfo[id.ToString()].Ping != 0){
                        continue;
                    }
                    DateTime utc = DateTime.UtcNow;
                    byte[] utc_b = MemoryPackSerializer.Serialize(utc);

                    EOSp2p.SendPacket((byte)CHANNELLIST.ObtainPing, utc_b, id);
                }
                await UniTask.Delay(500);

                if(refreshMembers == p2pInfo.Instance.userIds.RemoteUserIds.Count){
                    break;
                }
            }

            await UniTask.WhenAny(UniTask.WaitUntil(() => refreshMembers == p2pInfo.Instance.userIds.RemoteUserIds.Count, cancellationToken: token), UniTask.Delay(30000, cancellationToken: token));
            //Forced to end the count task.
            refreshMembers = p2pInfo.Instance.userIds.RemoteUserIds.Count;

            return true;
        }
        
        async UniTask RecivePongsPacket(CancellationToken token){
            while(!token.IsCancellationRequested){
                SugarPacket packet = p2pConnectorForOtherAssembly.Instance.GetPacketFromBuffer();

                if(packet != null){
                    if(packet.ch == (byte)CHANNELLIST.ObtainPing){
                        EOSp2p.SendPacket((byte)CHANNELLIST.ReturnPong, packet.payload, UserId.GetUserId(packet.UserID));
                    }else if(packet.ch == (byte)CHANNELLIST.ReturnPong){
                        GetPong(packet.UserID, packet.payload);
                    }
                }
                await UniTask.Yield();

                if(token.IsCancellationRequested || refreshMembers == p2pInfo.Instance.userIds.RemoteUserIds.Count){
                    break;
                }
            }
        }
    }
}