using PlayEveryWare.EpicOnlineServices;
using System;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Epic.OnlineServices.P2P;
using ResultE = Epic.OnlineServices.Result;

namespace SynicSugar.P2P {
    public static class EOSp2p {
        /// <summary>
        /// Add the header to this payload.
        /// </summary>
        public const int MAX_LARGEPACKET_PAYLOADSIZE = 1166;
        public const int MAX_LARGEPACKET_SIZE = 298496;
    #region Basic
        /// <summary>
        /// Send a packet to all remote peers. <br />
        /// Normally, <c>generated functions (have [RPC]) call this</c>, but we can also use this.
        /// We can SendPacketToAll(...).Forget()
        /// </summary>
        /// <param name="ch">(byte)ConnectHub.CHANNELLIST</param>
        /// <param name="value">Payload</param>
        public static async UniTaskVoid SendPacketToAll(byte ch, byte[] value){
            if(!p2pConfig.Instance.sessionCore.IsConnected){ return; }
            ArraySegment<byte> data = value is not null ? value : Array.Empty<byte>();
            ResultE result;
            int count = p2pConfig.Instance.RPCBatchSize;
            
        #if SYNICSUGAR_PACKETINFO
            Debug.Log($"SendPacket(All): ch {ch} / payload {ByteArrayToHexString(value)}");
        #endif
            foreach(var id in p2pInfo.Instance.userIds.RemoteUserIds){
                SendPacketOptions options = new SendPacketOptions(){
                    LocalUserId = EOSManager.Instance.GetProductUserId(),
                    RemoteUserId = id.AsEpic,
                    SocketId = p2pConfig.Instance.sessionCore.SocketId,
                    Channel = ch,
                    AllowDelayedDelivery = p2pConfig.Instance.AllowDelayedDelivery,
                    Reliability = p2pConfig.Instance.packetReliability,
                    Data = data
                };

                result = p2pConfig.Instance.sessionCore.P2PHandle.SendPacket(ref options);
                if(result != ResultE.Success){
                    Debug.LogErrorFormat("Send Packet: can't send packet, code: {0}", result);
                    continue;
                }

                count--;
                if(count <= 0){
                    await UniTask.Yield();
                    if(!p2pConfig.Instance.sessionCore.IsConnected){
                #if SYNICSUGAR_LOG
                        Debug.Log("Send Packet: get out of the loop by Cancel");
                #endif
                        break;
                    }
                    count = p2pConfig.Instance.RPCBatchSize;
                }
            }
        }
        /// <summary>
        /// Send a packet to all remote peers. <br />
        /// Normally, <c>generated functions (have [RPC]) call this</c>, but we can also use this.
        /// We can SendPacketToAll(...).Forget()
        /// </summary>
        /// <param name="ch">(byte)ConnectHub.CHANNELLIST</param>
        /// <param name="value">Payload</param>
        /// <param name="recordPacketInfo">If true, hold the last info in p2pInfo.</param>
        /// <returns></returns>
        public static async UniTaskVoid SendPacketToAll(byte ch, byte[] value, bool recordPacketInfo){
            if(!p2pConfig.Instance.sessionCore.IsConnected){ return; }
            ArraySegment<byte> data = value is not null ? value : Array.Empty<byte>();
            if(recordPacketInfo){
                p2pInfo.Instance.lastRpcInfo.ch = ch;
                p2pInfo.Instance.lastRpcInfo.payload = value;
                p2pInfo.Instance.lastRpcInfo.isLargePacket = true;
                // Current byte[] value is serialize just before call RPC, so we don't need to create new byte[] for this process.
                // No one can't use this "value" from elsewhere.
                // For readability and extensibility, we should create new array.
                // Now, we hold reference for performance.
                // ... = new byte[value.Length];
                // Array.Copy(value, p2pInfo.Instance.lastRpcInfo.payload, value.Length);
            }
            ResultE result;
            int count = p2pConfig.Instance.RPCBatchSize;

        #if SYNICSUGAR_PACKETINFO
            Debug.Log($"SendPacket(All): ch {ch} / payload {ByteArrayToHexString(value)}");
        #endif
            foreach(var id in p2pInfo.Instance.userIds.RemoteUserIds){
                SendPacketOptions options = new SendPacketOptions(){
                    LocalUserId = EOSManager.Instance.GetProductUserId(),
                    RemoteUserId = id.AsEpic,
                    SocketId = p2pConfig.Instance.sessionCore.SocketId,
                    Channel = ch,
                    AllowDelayedDelivery = p2pConfig.Instance.AllowDelayedDelivery,
                    Reliability = p2pConfig.Instance.packetReliability,
                    Data = data
                };

                result = p2pConfig.Instance.sessionCore.P2PHandle.SendPacket(ref options);
                if(result != ResultE.Success){
                    Debug.LogErrorFormat("Send Packet: can't send packet, code: {0}", result);
                    continue;
                }
                count--;
                if(count <= 0){
                    await UniTask.Yield();
                    if(!p2pConfig.Instance.sessionCore.IsConnected){
                #if SYNICSUGAR_LOG
                        Debug.Log("Send Packet: get out of the loop by Cancel");
                #endif
                        break;
                    }
                    count = p2pConfig.Instance.RPCBatchSize;
                }
            }
        }
        /// <summary>
        /// Send a packet to all remote peers. <br />
        /// Normally, <c>generated functions (have [RPC]) call this</c>, but we can also use this.
        /// We can SendPacketToAll(...).Forget()
        /// </summary>
        /// <param name="ch">(byte)ConnectHub.CHANNELLIST</param>
        /// <param name="value">Payload</param>
        /// <returns></returns>
        public static async UniTaskVoid SendPacketToAll(byte ch, ArraySegment<byte> value){
            if(!p2pConfig.Instance.sessionCore.IsConnected){ return; }
            ResultE result;
            int count = p2pConfig.Instance.RPCBatchSize;

        #if SYNICSUGAR_PACKETINFO
            Debug.Log($"SendPacket(ToAll): ch {ch} / payload {ByteArrayToHexString(value)}");
        #endif
            foreach(var id in p2pInfo.Instance.userIds.RemoteUserIds){
                SendPacketOptions options = new SendPacketOptions(){
                    LocalUserId = EOSManager.Instance.GetProductUserId(),
                    RemoteUserId = id.AsEpic,
                    SocketId = p2pConfig.Instance.sessionCore.SocketId,
                    Channel = ch,
                    AllowDelayedDelivery = p2pConfig.Instance.AllowDelayedDelivery,
                    Reliability = p2pConfig.Instance.packetReliability,
                    Data = value
                };

                result = p2pConfig.Instance.sessionCore.P2PHandle.SendPacket(ref options);
                if(result != ResultE.Success){
                    Debug.LogErrorFormat("Send Packet: can't send packet, code: {0}", result);
                    continue;
                }
                count--;
                if(count <= 0){
                    await UniTask.Yield();
                    if(!p2pConfig.Instance.sessionCore.IsConnected){
                #if SYNICSUGAR_LOG
                        Debug.Log("Send Packet: get out of the loop by Cancel");
                #endif
                        break;
                    }
                    count = p2pConfig.Instance.RPCBatchSize;
                }
            }
        }
        /// <summary>
        /// Send a packet to a specific peer.
        /// </summary>
        /// <param name="ch">(byte)ConnectHub.CHANNELLIST</param>
        /// <param name="value">Payload</param>
        /// <param name="targetId">Target to send</param>
        public static void SendPacket(byte ch, byte[] value, UserId targetId){
            if(!p2pConfig.Instance.sessionCore.IsConnected){ return; }
            SendPacketOptions options = new SendPacketOptions(){
                LocalUserId = EOSManager.Instance.GetProductUserId(),
                RemoteUserId = targetId.AsEpic,
                SocketId = p2pConfig.Instance.sessionCore.SocketId,
                Channel = ch,
                AllowDelayedDelivery = p2pConfig.Instance.AllowDelayedDelivery,
                Reliability = p2pConfig.Instance.packetReliability,
                Data = new ArraySegment<byte>(value != null ? value : Array.Empty<byte>())
            };
        #if SYNICSUGAR_PACKETINFO
            Debug.Log($"SendPacket: ch {ch} / payload {ByteArrayToHexString(value)}");
        #endif
            ResultE result = p2pConfig.Instance.sessionCore.P2PHandle.SendPacket(ref options);

            if(result != ResultE.Success){
                Debug.LogErrorFormat("Send Packet: can't send packet, code: {0}", result);
                return;
            }
        }
        /// <summary>
        /// Send a packet to a specific peer.
        /// </summary>
        /// <param name="ch">(byte)ConnectHub.CHANNELLIST</param>
        /// <param name="value">Payload</param>
        /// <param name="targetId">Target to send</param>
        /// <param name="recordPacketInfo">If true, hold the last info in p2pInfo.</param>
        public static void SendPacket(byte ch, byte[] value, UserId targetId, bool recordPacketInfo){
            if(!p2pConfig.Instance.sessionCore.IsConnected){ return; }
            if(recordPacketInfo){
                p2pInfo.Instance.lastTargetRPCInfo.ch = ch;
                p2pInfo.Instance.lastTargetRPCInfo.payload = value;
                p2pInfo.Instance.lastTargetRPCInfo.target = targetId;
                p2pInfo.Instance.lastTargetRPCInfo.isLargePacket = false;
            }
            SendPacketOptions options = new SendPacketOptions(){
                LocalUserId = EOSManager.Instance.GetProductUserId(),
                RemoteUserId = targetId.AsEpic,
                SocketId = p2pConfig.Instance.sessionCore.SocketId,
                Channel = ch,
                AllowDelayedDelivery = p2pConfig.Instance.AllowDelayedDelivery,
                Reliability = p2pConfig.Instance.packetReliability,
                Data = new ArraySegment<byte>(value != null ? value : Array.Empty<byte>())
            };
        #if SYNICSUGAR_PACKETINFO
            Debug.Log($"SendPacket: ch {ch} / payload {ByteArrayToHexString(value)}");
        #endif
            ResultE result = p2pConfig.Instance.sessionCore.P2PHandle.SendPacket(ref options);

            if(result != ResultE.Success){
                Debug.LogErrorFormat("Send Packet: can't send packet, code: {0}", result);
                return;
            }
        }
        
        /// <summary>
        /// Send a packet to a specific peer.
        /// </summary>
        /// <param name="ch">(byte)ConnectHub.CHANNELLIST</param>
        /// <param name="value">Payload</param>
        /// <param name="targetId">Target to send</param>
        public static void SendPacket(byte ch, ArraySegment<byte> value, UserId targetId){
            if(!p2pConfig.Instance.sessionCore.IsConnected){ return; }
            SendPacketOptions options = new SendPacketOptions(){
                LocalUserId = EOSManager.Instance.GetProductUserId(),
                RemoteUserId = targetId.AsEpic,
                SocketId = p2pConfig.Instance.sessionCore.SocketId,
                Channel = ch,
                AllowDelayedDelivery = p2pConfig.Instance.AllowDelayedDelivery,
                Reliability = p2pConfig.Instance.packetReliability,
                Data = value
            };
        #if SYNICSUGAR_PACKETINFO
            Debug.Log($"SendPacket: ch {ch} / payload {ByteArrayToHexString(value)}");
        #endif
            ResultE result = p2pConfig.Instance.sessionCore.P2PHandle.SendPacket(ref options);

            if(result != ResultE.Success){
                Debug.LogErrorFormat("Send Packet: can't send packet, code: {0}", result);
                return;
            }
        }
    #endregion
    #region LargePacket
        /// <summary>
        /// Send TargetRPC as LargePacket. <br />
        /// </summary>
        /// <param name="ch"></param>
        /// <param name="value"></param>
        /// <param name="targetId"></param>
        public async static UniTask SendLargePackets(byte ch, byte[] value, UserId targetId){
            if(!p2pConfig.Instance.sessionCore.IsConnected){ return; }
            if(value.Length > MAX_LARGEPACKET_SIZE){
                throw new ArgumentException("SendPacket: Data size exceeds maximum large packet size");
            }
            int length = MAX_LARGEPACKET_PAYLOADSIZE;
            byte[] header = GenerateHeader(value.Length);

        #if SYNICSUGAR_PACKETINFO
            Debug.Log($"SendPacket(Large): ch {ch} / payload size {value.Length} / additional packets {header[1]}");
        #endif

            //Max payload is 1170 but we need some header.
            for(int startIndex = 0; startIndex < value.Length; startIndex += MAX_LARGEPACKET_PAYLOADSIZE){
                length = startIndex + MAX_LARGEPACKET_PAYLOADSIZE < value.Length ? MAX_LARGEPACKET_PAYLOADSIZE : value.Length - startIndex;
                SendPacket(value, startIndex, length, header, targetId, ch);
                header[0]++;
                //For sending buffer and main thread fps.
                if(header[0] % p2pConfig.Instance.LargePacketBatchSize == 0){
                    await UniTask.Yield();
                }

            }
        #if SYNICSUGAR_LOG
            Debug.Log($"SendLargePackets: Finish to Send to {targetId}!");
        #endif
        }
        
        /// <summary>
        /// Send TargetRPC as LargePacket. <br />
        /// </summary>
        /// <param name="ch"></param>
        /// <param name="value"></param>
        /// <param name="targetId"></param>
        public async static UniTask SendLargePackets(byte ch, byte[] value, UserId targetId, bool recordPacketInfo){
            if(!p2pConfig.Instance.sessionCore.IsConnected){ return; }
            if(value.Length > MAX_LARGEPACKET_SIZE){
                throw new ArgumentException("SendPacket(Large): Data size exceeds maximum large packet size");
            }
            if(recordPacketInfo){
                p2pInfo.Instance.lastTargetRPCInfo.ch = ch;
                p2pInfo.Instance.lastTargetRPCInfo.payload = value;
                p2pInfo.Instance.lastTargetRPCInfo.target = targetId;
                p2pInfo.Instance.lastTargetRPCInfo.isLargePacket = true;
            }

            int length = MAX_LARGEPACKET_PAYLOADSIZE;
            byte[] header = GenerateHeader(value.Length);

        #if SYNICSUGAR_PACKETINFO
            Debug.Log($"SendPacket(Large): ch {ch} / payload size {value.Length} / additional packets {header[1]}");
        #endif

            //Max payload is 1170 but we need some header.
            for(int startIndex = 0; startIndex < value.Length; startIndex += MAX_LARGEPACKET_PAYLOADSIZE){
                length = startIndex + MAX_LARGEPACKET_PAYLOADSIZE < value.Length ? MAX_LARGEPACKET_PAYLOADSIZE : value.Length - startIndex;
                SendPacket(value, startIndex, length, header, targetId, ch);
                header[0]++;
                //For sending buffer and main thread fps.
                if(header[0] % p2pConfig.Instance.LargePacketBatchSize == 0){
                    await UniTask.Yield();
                }

            }
        #if SYNICSUGAR_LOG
            Debug.Log($"SendLargePackets: Finish to Send to {targetId}!");
        #endif
        }
        //To use Span. However, this process generates Garbage by each loop.
        static void SendPacket(byte[] value, int startIndex, int length, byte[] header, UserId targetId, byte ch){
            if(!p2pConfig.Instance.sessionCore.IsConnected){ return; }
            Span<byte> _payload = new Span<byte>(value, startIndex, length); 
            //Add header
            Span<byte> payload = new byte[header.Length + length];
            header.CopyTo(payload);
            _payload.CopyTo(payload.Slice(2));

            //LargePacket has Dictionary for each Ch and restores the packets when all packets are received and deserialize packet as one packet.
            //So, that must be ReliableOrdered to avoid mixing packets.
            SendPacketOptions options = new SendPacketOptions(){
                LocalUserId = EOSManager.Instance.GetProductUserId(),
                RemoteUserId = targetId.AsEpic,
                SocketId = p2pConfig.Instance.sessionCore.SocketId,
                Channel = ch,
                AllowDelayedDelivery = true,
                Reliability = PacketReliability.ReliableOrdered, //Fixed
                Data = new ArraySegment<byte>(payload.ToArray())
            };

            ResultE result = p2pConfig.Instance.sessionCore.P2PHandle.SendPacket(ref options);

            if(result != ResultE.Success){
                Debug.LogErrorFormat("Send Large Packet: can't send packet, code: {0}", result);
                return;
            }
        }
        /// <summary>
        /// Send RPC as LargePacket. 
        /// </summary>
        /// <param name="ch"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public async static UniTask SendLargePacketsToAll(byte ch, byte[] value){
            if(!p2pConfig.Instance.sessionCore.IsConnected){ return; }
            if(value.Length > MAX_LARGEPACKET_SIZE){
                throw new ArgumentException("SendPacket(Large/All): Data size exceeds maximum large packet size");
            }
            foreach(var id in p2pInfo.Instance.userIds.RemoteUserIds){
                await SendLargePackets(ch, value, id);
            }
        #if SYNICSUGAR_LOG
            Debug.Log($"Finish SendLargePacketsToAll for {ch}");
        #endif
        }
        
        /// <summary>
        /// Send RPC as LargePacket. <br />
        /// </summary>
        /// <param name="ch"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public async static UniTask SendLargePacketsToAll(byte ch, byte[] value, bool recordPacketInfo){
            if(!p2pConfig.Instance.sessionCore.IsConnected){ return; }
            if(value.Length > MAX_LARGEPACKET_SIZE){
                throw new ArgumentException("SendPacket(Large/ALL): Data size exceeds maximum large packet size");
            }
            if(recordPacketInfo){
                p2pInfo.Instance.lastRpcInfo.ch = ch;
                p2pInfo.Instance.lastRpcInfo.payload = value;
                p2pInfo.Instance.lastRpcInfo.isLargePacket = true;
                // Current byte[] value is serialize just before call RPC, so we don't need to create new byte[] for this process.
                // No one can't use this "value" from elsewhere.
                // For readability and extensibility, we should create new array.
                // Now, we hold reference for performance.
                // ... = new byte[value.Length];
                // Array.Copy(value, p2pInfo.Instance.lastRpcInfo.payload, value.Length);
            }
            foreach(var id in p2pInfo.Instance.userIds.RemoteUserIds){
                await SendLargePackets(ch, value, id);
            }
        #if SYNICSUGAR_LOG
            Debug.Log($"Finish SendLargePacketsToAll for {ch}");
        #endif
        }
    #endregion

    #region For Synic
        /// <summary>
        /// Send a synic packet to a specific peer. Main uses is to send hame data to returner. <br />
        /// Add header to sent divided packets.
        /// *Current: We can use this from a specific API.
        /// </summary>
        /// <param name="ch">Only 255 now</param>
        /// <param name="value">The payload serialized with MemoryPack and BrotliCompressor</param>
        /// <param name="targetId"></param>
        /// <param name="dataOwner">Is it whose data?</param>
        /// <param name="syncedPhase">Sync from 0 to hierarchy</param>
        /// <param name="syncSpecificPhase">If false, synchronize an only specific hierarchy</param>
        public static void SendSynicPackets(byte ch, byte[] value, UserId targetId, UserId dataOwner, byte syncedPhase = 9, bool syncSpecificPhase = false){
            if(!p2pConfig.Instance.sessionCore.IsConnected){ return; }
            if(value.Length > MAX_LARGEPACKET_SIZE){
                throw new ArgumentException("SendPacket(Synic): Data size exceeds maximum large packet size");
            }
            int length = MAX_LARGEPACKET_PAYLOADSIZE;
            byte[] header = GenerateHeader(value.Length, syncedPhase, syncSpecificPhase, targetId, dataOwner);

        #if SYNICSUGAR_PACKETINFO
            Debug.Log($"SendPacket(Synic): payload size {value.Length} / chunk {header[1]} / hierarchy {header[2]} / syncSpecificPhase {header[3]}");
        #endif

            //Max payload is 1170 but we need some header.
            for(int startIndex = 0; startIndex < value.Length; startIndex += MAX_LARGEPACKET_PAYLOADSIZE){
                length = startIndex + MAX_LARGEPACKET_PAYLOADSIZE < value.Length ? MAX_LARGEPACKET_PAYLOADSIZE : value.Length - startIndex;

                Span<byte> _payload = new Span<byte>(value, startIndex, length); 
                //Add header
                Span<byte> payload = new byte[header.Length + length];
                header.CopyTo(payload);
                _payload.CopyTo(payload.Slice(4));

                //LargePacket has Dictionary for each Ch and restores the packets when all packets are received and deserialize packet as one packet.
                //So, that must be ReliableOrdered to avoid mixing packets.
                SendPacketOptions options = new SendPacketOptions(){
                    LocalUserId = EOSManager.Instance.GetProductUserId(),
                    RemoteUserId = targetId.AsEpic,
                    SocketId = p2pConfig.Instance.sessionCore.SocketId,
                    Channel = ch,
                    AllowDelayedDelivery = true,
                    Reliability = PacketReliability.ReliableOrdered, //Fixed
                    Data = new ArraySegment<byte>(payload.ToArray())
                };

                ResultE result = p2pConfig.Instance.sessionCore.P2PHandle.SendPacket(ref options);

                if(result != ResultE.Success){
                    Debug.LogErrorFormat("Send Large Packet: can't send packet, code: {0}", result);
                    return;
                }
                //add index
                header[0]++;
            }
        #if SYNICSUGAR_LOG
            Debug.Log($"Send Large Packet: Success to {targetId}!");
        #endif
        }
    #endregion
    #region Large-packet header
        /// <summary>
        /// 0-packet index, 1-additional packet amount
        /// </summary>
        static byte[] GenerateHeader(int valueLength){
            byte[] header = new byte[2];
            //packet index
            header[0] = 0;
            //additional packet amount
            header[1] = (byte)((valueLength -1) / MAX_LARGEPACKET_PAYLOADSIZE); 
            return header;
        }
        /// <summary>
        /// 0-packet index, 1-additional packet amount, 2-complex data[1bit-isOnly, 4bits-phase, 3bits userType], 3-Data's user index
        /// </summary>
        static byte[] GenerateHeader(int valueLength, byte phase, bool isOnly, UserId target, UserId dataOwner){
            byte[] header = new byte[4];
            //packet index
            header[0] = 0;
            //additional packet amount
            header[1] = (byte)((valueLength -1) / MAX_LARGEPACKET_PAYLOADSIZE);
            //complex data
            int userType;
            if(p2pInfo.Instance.IsLoaclUser(dataOwner)){
                userType = 1; //Local
            }else if(target == dataOwner){
                userType = 0; //Target self
            }else{
                userType = 2; //Others
            }

            header[2] = (byte)(
                ((isOnly ? 1 : 0) << 7) |  // 1bit (0-1)
                ((phase & 0x0F) << 3) |    // 4bit (0-15)
                (userType & 0x07)          // 3bit (0-7)
            );
            //Data's user index
            header[3] = userType == 2 ? (byte)p2pInfo.Instance.AllUserIds.IndexOf(dataOwner) : (byte)0;

            return header;
        }
    #endregion
#if SYNICSUGAR_PACKETINFO
    /// <summary>
    /// Convert byte to String.
    /// </summary>
    /// <param name="byteArray"></param>
    /// <returns></returns>
    internal static string ByteArrayToHexString(byte[] byteArray) {
        if(byteArray == null){
            return string.Empty;
        }
        System.Text.StringBuilder hex = new System.Text.StringBuilder(byteArray.Length * 2);
        foreach (byte b in byteArray) {
            hex.AppendFormat("{0:x2}", b);
        }
        return hex.ToString();
    }
    /// <summary>
    /// Convert byte to String.
    /// </summary>
    /// <param name="byteArray"></param>
    /// <returns></returns>
    internal static string ByteArrayToHexString(ArraySegment<byte> byteArray){
        if(byteArray.Count == 0){
            return string.Empty;
        }
        System.Text.StringBuilder hex = new System.Text.StringBuilder(byteArray.Count * 2);
        for (int i = byteArray.Offset; i < byteArray.Offset + byteArray.Count; i++){
            hex.AppendFormat("{0:x2}", byteArray.Array[i]);
        }
        return hex.ToString();
    }
#endif
    }
}