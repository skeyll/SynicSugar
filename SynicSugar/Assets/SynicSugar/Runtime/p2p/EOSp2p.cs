using PlayEveryWare.EpicOnlineServices;
using System;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Epic.OnlineServices.P2P;
using ResultE = Epic.OnlineServices.Result;

namespace SynicSugar.P2P {
    public static class EOSp2p {
        /// <summary>
        /// Send a packet to all remote peers. <br />
        /// Normally, <c>generated functions (have [RPC]) call this</c>, but we can also use this.
        /// We can SendPacketToAll(...).Forget()
        /// </summary>
        /// <param name="ch"></param>
        /// <param name="value"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async UniTaskVoid SendPacketToAll(byte ch, byte[] value){
            SendPacketOptions options = new SendPacketOptions(){
                LocalUserId = EOSManager.Instance.GetProductUserId(),
                SocketId = p2pConnectorForOtherAssembly.Instance.SocketId,
                Channel = ch,
                AllowDelayedDelivery = false,
                Reliability = PacketReliability.ReliableOrdered,
                Data = new ArraySegment<byte>(value != null ? value : Array.Empty<byte>())
            };

            ResultE result;
            foreach(var id in p2pConfig.Instance.userIds.RemoteUserIds){
                options.RemoteUserId = id.AsEpic;
                result = p2pConnectorForOtherAssembly.Instance.P2PHandle.SendPacket(ref options);

                await UniTask.Delay(p2pConfig.Instance.interval_sendToAll);
                if(result != ResultE.Success){
                    Debug.LogErrorFormat("Send Packet: can't send packet, code: {0}", result);
                    break;
                }
                if(p2pConnectorForOtherAssembly.Instance.p2pToken.IsCancellationRequested){
            #if SYNICSUGAR_LOG
                    Debug.Log("Send Packet: get out of the loop by Cancel");
            #endif
                    break;
                }
            }
        }
        /// <summary>
        /// Send a packet to a specific peer.
        /// </summary>
        /// <param name="ch"></param>
        /// <param name="value"></param>
        /// <param name="targetId"></param>
        public static void SendPacket(byte ch, byte[] value, UserId targetId){
            SendPacketOptions options = new SendPacketOptions(){
                LocalUserId = EOSManager.Instance.GetProductUserId(),
                RemoteUserId = targetId.AsEpic,
                SocketId = p2pConnectorForOtherAssembly.Instance.SocketId,
                Channel = ch,
                AllowDelayedDelivery = false,
                Reliability = p2pConfig.Instance.packetReliability,
                Data = new ArraySegment<byte>(value != null ? value : Array.Empty<byte>())
            };

            ResultE result = p2pConnectorForOtherAssembly.Instance.P2PHandle.SendPacket(ref options);

            if(result != ResultE.Success){
                Debug.LogErrorFormat("Send Packet: can't send packet, code: {0}", result);
                return;
            }
        }
        /// <summary>
        /// Send a large packet to a specific peer. To send returner. </ br>
        /// Add header to sent divided packets.
        /// *Current: We can use this from a specific API.
        /// </summary>
        /// <param name="ch">Only 255 now</param>
        /// <param name="value">The payload serialized with MemoryPack and BrotliCompressor</param>
        /// <param name="targetId"></param>
        /// <param name="hierarchyLevel">Sync from 0 to hierarchy</param>
        /// <param name="syncAllHierarchy">If false, synchronize an only specific hierarchy</param>
        public static void SendLargePacket(byte ch, byte[] value, UserId targetId, byte hierarchyLevel = 255, bool syncSpecificHierarchy = false){
            int length = 1100;
            byte chunkIndex = 0;
            byte chunkAmount = (byte)Math.Ceiling(value.Length / 1100f);
            //Max payload is 1170 but we need some header.
            for(int startIndex = 0; startIndex < value.Length; startIndex += 1100){
                length = startIndex + 1100 < value.Length ? 1100 : value.Length - startIndex;

                Span<byte> _payload = new Span<byte>(value, startIndex, length); 
                byte[] header = GenerateHeader(chunkAmount, hierarchyLevel, syncSpecificHierarchy, ref chunkIndex);
                //Add header
                byte[] payload = new byte[header.Length + length];
                Array.Copy(header, 0, payload, 0, header.Length);
                Array.Copy(_payload.ToArray(), 0, payload, header.Length, length);

                SendPacketOptions options = new SendPacketOptions(){
                    LocalUserId = EOSManager.Instance.GetProductUserId(),
                    RemoteUserId = targetId.AsEpic,
                    SocketId = p2pConnectorForOtherAssembly.Instance.SocketId,
                    Channel = ch,
                    AllowDelayedDelivery = true,
                    Reliability = p2pConfig.Instance.packetReliability,
                    Data = new ArraySegment<byte>(payload)
                };

                ResultE result = p2pConnectorForOtherAssembly.Instance.P2PHandle.SendPacket(ref options);

                if(result != ResultE.Success){
                    Debug.LogErrorFormat("Send Large Packet: can't send packet, code: {0}", result);
                    return;
                }
            }
        #if SYNICSUGAR_LOG
            Debug.Log($"Send Large Packet: Success to {targetId.ToString()}!");
        #endif
            /// <summary>
            /// index, (chunk count, hierarchy level, only sync hierarchy), tail
            /// </summary>
            byte[] GenerateHeader(byte chunk, byte hierarchy, bool isOnly, ref byte chunkIndex){
                byte[] result = new byte[chunkIndex == 0 ? 5 : 2];

                result[0] = chunkIndex;
                if(chunkIndex == 0){
                    result[1] = chunk;
                    result[2] = hierarchy;
                    result[3] = isOnly ? (byte)1 : (byte)0;
                }
                result[result.Length - 1] = 0;
                chunkIndex++;

                return result;
            }
        }
    }
    public enum Reason {
        Left, Disconnected, Unknown
    }
}
