using PlayEveryWare.EpicOnlineServices;
using Epic.OnlineServices.P2P;
using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
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
    }
    public enum Reason {
        Left, Disconnected, Unknown
    }
}
