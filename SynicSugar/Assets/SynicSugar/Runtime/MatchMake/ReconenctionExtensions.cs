using System;
using System.Threading;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using SynicSugar.P2P;
using MemoryPack;
using MemoryPack.Compression;
using PlayEveryWare.EpicOnlineServices;
using Epic.OnlineServices;
using Epic.OnlineServices.P2P;
using ResultE = Epic.OnlineServices.Result;

namespace SynicSugar.MatchMake {
    internal class ReconenctionExtensions {
        /// <summary>
        /// Different Assembly can have same CH, but not sorted when receive packet. <br />
        /// So must not use the same ch for what SynicSugar may receive at the same time.
        /// </summary>
        const byte RECONNECTIONCH = 252;
        byte ch;
        string id;
        ArraySegment<byte> payload;
        /// <summary>
        /// For Host to send List after reconnecter has came.
        /// </summary>
        internal static void SendUserLists(UserId target){
            List<string> tmp = new List<string>();
            foreach(var id in p2pInfo.Instance.AllUserIds){
                tmp.Add(id.ToString());
            }
            
            using var compressor  = new BrotliCompressor();
            MemoryPackSerializer.Serialize(compressor, tmp);
            EOSp2p.SendPacket(RECONNECTIONCH, compressor.ToArray(), target);
        }
        internal async UniTask ReciveUserIdsPacket(CancellationToken token){
            while(!token.IsCancellationRequested){
                bool recivePacket = GetPacketFromBuffer(ref ch, ref id, ref payload);

                if(recivePacket){
                    ConvertFromPacket();
                    return;
                }
                await UniTask.Yield();
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
                RequestedChannel = ch
            };
            //Next packet size
            var getNextReceivedPacketSizeOptions = new GetNextReceivedPacketSizeOptions {
                LocalUserId = p2pInfo.Instance.userIds.LocalUserId.AsEpic,
                RequestedChannel = ch
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
            if(ch != RECONNECTIONCH){
                return;
            }

            using var decompressor = new BrotliDecompressor();
            var decompressed = decompressor.Decompress(payload);

            List<string> data = MemoryPackSerializer.Deserialize<List<string>>(decompressed);
            p2pInfo.Instance.userIds.OverwriteAllUserIdsWithOrdered(data);
        }
    }
}
