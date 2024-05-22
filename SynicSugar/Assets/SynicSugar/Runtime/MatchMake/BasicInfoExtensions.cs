using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using SynicSugar.P2P;
using MemoryPack;
using MemoryPack.Compression;
using PlayEveryWare.EpicOnlineServices;
using Epic.OnlineServices;
using Epic.OnlineServices.P2P;
using ResultE = Epic.OnlineServices.Result;
using UnityEngine;

namespace SynicSugar.MatchMake {
    internal class BasicInfoExtensions {
        /// <summary>
        /// Different Assembly can have same CH, but not sorted when receive packet. <br />
        /// So must not use the same ch for what SynicSugar may receive at the same time.
        /// </summary>
        const byte USERLISTCH = 252;
        byte ch;
        string id;
        ArraySegment<byte> payload;
        #region Send
        /// <summary>
        /// For Host to send List after re-connecter has came.
        /// </summary>
        internal static void SendUserList(UserId target){
            BasicInfo basicInfo = new BasicInfo();
            basicInfo.userIds = p2pInfo.Instance.AllUserIds.ConvertAll(id => id.ToString());
            
            if(p2pInfo.Instance.DisconnectedUserIds.Count > 0){
                for(int i = 0; i < p2pInfo.Instance.DisconnectedUserIds.Count; i++){
                    basicInfo.disconnectedUserIndexes.Add(p2pInfo.Instance.AllUserIds.IndexOf(p2pInfo.Instance.DisconnectedUserIds[i]));
                }
            }

            using var compressor  = new BrotliCompressor();
            MemoryPackSerializer.Serialize(compressor, basicInfo);
            SendPacket(USERLISTCH, compressor.ToArray(), target);
        }
        /// <summary>
        /// For Host to send AllUserList after connection.
        /// </summary>
        internal static async UniTask SendUserListToAll(CancellationToken token){
            BasicInfo basicInfo = new BasicInfo();
            basicInfo.userIds = p2pInfo.Instance.AllUserIds.ConvertAll(id => id.ToString());
            
            using var compressor  = new BrotliCompressor();
            MemoryPackSerializer.Serialize(compressor, basicInfo);
            
            int count = p2pConfig.Instance.RPCBatchSize;
            var compressorArray = compressor.ToArray();
            foreach(var id in p2pInfo.Instance.userIds.RemoteUserIds){
                SendPacket(USERLISTCH, compressorArray, id);

                count--;
                if(count <= 0){
                await UniTask.Yield(cancellationToken: token);
                    if(token.IsCancellationRequested){
                #if SYNICSUGAR_LOG
                        Debug.Log("SendUserListToAll: get out of the loop by Cancel");
                #endif
                        break;
                    }
                    count = p2pConfig.Instance.RPCBatchSize;
                }
            }
        }
        
        static void SendPacket(byte ch, byte[] value, UserId targetId){
            SendPacketOptions options = new SendPacketOptions(){
                LocalUserId = EOSManager.Instance.GetProductUserId(),
                RemoteUserId = targetId.AsEpic,
                SocketId = p2pConnectorForOtherAssembly.Instance.SocketId,
                Channel = ch,
                AllowDelayedDelivery = true,
                Reliability = PacketReliability.ReliableOrdered,
                Data = new ArraySegment<byte>(value)
            };

            P2PInterface P2PHandle = EOSManager.Instance.GetEOSPlatformInterface().GetP2PInterface();
            ResultE result = P2PHandle.SendPacket(ref options);

            if(result != ResultE.Success){
                Debug.LogErrorFormat("SendUserLists: can't send packet, code: {0}", result);
                return;
            }
        }
        #endregion
        #region Receive
        internal async UniTask ReciveUserIdsPacket(CancellationToken token){
            while(!token.IsCancellationRequested){
                bool recivePacket = GetPacketFromBuffer(ref ch, ref id, ref payload);

                if(recivePacket){
                    ConvertFromPacket();
                    return;
                }
                await UniTask.Yield(cancellationToken: token);
            }
        }
        /// <summary>
        /// To get RECONNECTIONCH packet.
        /// </summary>
        /// <param name="ch"></param>
        /// <param name="id"></param>
        /// <param name="payload"></param>
        /// <returns></returns>
        bool GetPacketFromBuffer(ref byte ch, ref string id, ref ArraySegment<byte> payload){
            //Set options
            ReceivePacketOptions options = new ReceivePacketOptions(){
                LocalUserId = p2pInfo.Instance.userIds.LocalUserId.AsEpic,
                MaxDataSizeBytes = 1170,
                RequestedChannel = USERLISTCH
            };
            //Next packet size
            var getNextReceivedPacketSizeOptions = new GetNextReceivedPacketSizeOptions {
                LocalUserId = p2pInfo.Instance.userIds.LocalUserId.AsEpic,
                RequestedChannel = USERLISTCH
            };

            P2PInterface P2PHandle = EOSManager.Instance.GetEOSPlatformInterface().GetP2PInterface();
            P2PHandle.GetNextReceivedPacketSize(ref getNextReceivedPacketSizeOptions, out uint nextPacketSizeBytes);

            byte[] data = new byte[nextPacketSizeBytes];
            var dataSegment = new ArraySegment<byte>(data);
            ResultE result = P2PHandle.ReceivePacket(ref options, out ProductUserId peerId, out SocketId socketId, out byte outChannel, dataSegment, out uint bytesWritten);
            
            if (result != ResultE.Success){
                return false; //No packet
            }
            ch = outChannel;
            id = peerId.ToString();
            payload = new ArraySegment<byte>(dataSegment.Array, dataSegment.Offset, (int)bytesWritten);;

            return true;
        }
        
        void ConvertFromPacket(){
            if(ch != USERLISTCH){
                return;
            }

            using var decompressor = new BrotliDecompressor();
            var decompressed = decompressor.Decompress(payload);

            BasicInfo data = MemoryPackSerializer.Deserialize<BasicInfo>(decompressed);
            p2pInfo.Instance.userIds.OverwriteAllUserIdsWithOrdered(data);
        }
        #endregion
    }
}
