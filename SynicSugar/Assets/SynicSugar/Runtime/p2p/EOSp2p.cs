using PlayEveryWare.EpicOnlineServices;
using Epic.OnlineServices;
using Epic.OnlineServices.P2P;
using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace SynicSugar.P2P {
    public static class EOSp2p {
        /// <summary>
        /// Send packet to all remote pears. <br />
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
                AllowDelayedDelivery = true,
                Reliability = PacketReliability.ReliableOrdered,
                Data = new ArraySegment<byte>(value != null ? value : Array.Empty<byte>())
            };

            Result result;
            foreach(var id in p2pConfig.Instance.userIds.RemoteUserIds){
                options.RemoteUserId = id.AsEpic;
                result = p2pConnectorForOtherAssembly.Instance.P2PHandle.SendPacket(ref options);

                await UniTask.Delay(p2pConfig.Instance.interval_sendToAll);
                if(result != Result.Success){
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
        /// Send packet to a specific pear.
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
                AllowDelayedDelivery = true,
                Reliability = p2pConfig.Instance.packetReliability,
                Data = new ArraySegment<byte>(value != null ? value : Array.Empty<byte>())
            };

            Result result = p2pConnectorForOtherAssembly.Instance.P2PHandle.SendPacket(ref options);

            if(result != Result.Success){
                Debug.LogErrorFormat("Send Packet: can't send packet, code: {0}", result);
                return;
            }
        }
    }
}
