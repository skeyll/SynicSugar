using PlayEveryWare.EpicOnlineServices;
using System;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Epic.OnlineServices.P2P;
using ResultE = Epic.OnlineServices.Result;

namespace SynicSugar.P2P {
    public static class EOSp2p {
    #region Basic
        /// <summary>
        /// Send a packet to all remote peers. <br />
        /// Normally, <c>generated functions (have [RPC]) call this</c>, but we can also use this.
        /// We can SendPacketToAll(...).Forget()
        /// </summary>
        /// <param name="ch">(byte)ConnectHub.CHANNELLIST</param>
        /// <param name="value">Payload</param>
        public static async UniTaskVoid SendPacketToAll(byte ch, byte[] value){
            ArraySegment<byte> data = value is not null ? value : Array.Empty<byte>();
            ResultE result;
            int count = p2pConfig.Instance.RPCBatchSize;
            foreach(var id in p2pInfo.Instance.userIds.RemoteUserIds){
                SendPacketOptions options = new SendPacketOptions(){
                    LocalUserId = EOSManager.Instance.GetProductUserId(),
                    RemoteUserId = id.AsEpic,
                    SocketId = p2pConnectorForOtherAssembly.Instance.SocketId,
                    Channel = ch,
                    AllowDelayedDelivery = p2pConfig.Instance.AllowDelayedDelivery,
                    Reliability = PacketReliability.ReliableOrdered,
                    Data = data
                };

                result = p2pConnectorForOtherAssembly.Instance.P2PHandle.SendPacket(ref options);
                if(result != ResultE.Success){
                    Debug.LogErrorFormat("Send Packet: can't send packet, code: {0}", result);
                    continue;
                }

                count--;
                if(count <= 0){
                    await UniTask.Yield();
                    if(p2pConnectorForOtherAssembly.Instance.p2pToken != null && p2pConnectorForOtherAssembly.Instance.p2pToken.IsCancellationRequested){
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
            foreach(var id in p2pInfo.Instance.userIds.RemoteUserIds){
                SendPacketOptions options = new SendPacketOptions(){
                    LocalUserId = EOSManager.Instance.GetProductUserId(),
                    RemoteUserId = id.AsEpic,
                    SocketId = p2pConnectorForOtherAssembly.Instance.SocketId,
                    Channel = ch,
                    AllowDelayedDelivery = p2pConfig.Instance.AllowDelayedDelivery,
                    Reliability = PacketReliability.ReliableOrdered,
                    Data = data
                };

                result = p2pConnectorForOtherAssembly.Instance.P2PHandle.SendPacket(ref options);
                if(result != ResultE.Success){
                    Debug.LogErrorFormat("Send Packet: can't send packet, code: {0}", result);
                    continue;
                }
                count--;
                if(count <= 0){
                    await UniTask.Yield();
                    if(p2pConnectorForOtherAssembly.Instance.p2pToken != null && p2pConnectorForOtherAssembly.Instance.p2pToken.IsCancellationRequested){
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
            ResultE result;
            int count = p2pConfig.Instance.RPCBatchSize;
            foreach(var id in p2pInfo.Instance.userIds.RemoteUserIds){
                SendPacketOptions options = new SendPacketOptions(){
                    LocalUserId = EOSManager.Instance.GetProductUserId(),
                    RemoteUserId = id.AsEpic,
                    SocketId = p2pConnectorForOtherAssembly.Instance.SocketId,
                    Channel = ch,
                    AllowDelayedDelivery = p2pConfig.Instance.AllowDelayedDelivery,
                    Reliability = PacketReliability.ReliableOrdered,
                    Data = value
                };

                result = p2pConnectorForOtherAssembly.Instance.P2PHandle.SendPacket(ref options);
                if(result != ResultE.Success){
                    Debug.LogErrorFormat("Send Packet: can't send packet, code: {0}", result);
                    continue;
                }
                count--;
                if(count <= 0){
                    await UniTask.Yield();
                    if(p2pConnectorForOtherAssembly.Instance.p2pToken != null && p2pConnectorForOtherAssembly.Instance.p2pToken.IsCancellationRequested){
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
            SendPacketOptions options = new SendPacketOptions(){
                LocalUserId = EOSManager.Instance.GetProductUserId(),
                RemoteUserId = targetId.AsEpic,
                SocketId = p2pConnectorForOtherAssembly.Instance.SocketId,
                Channel = ch,
                AllowDelayedDelivery = p2pConfig.Instance.AllowDelayedDelivery,
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
        /// Send a packet to a specific peer.
        /// </summary>
        /// <param name="ch">(byte)ConnectHub.CHANNELLIST</param>
        /// <param name="value">Payload</param>
        /// <param name="targetId">Target to send</param>
        /// <param name="recordPacketInfo">If true, hold the last info in p2pInfo.</param>
        public static void SendPacket(byte ch, byte[] value, UserId targetId, bool recordPacketInfo){
            if(recordPacketInfo){
                p2pInfo.Instance.lastTargetRPCInfo.ch = ch;
                p2pInfo.Instance.lastTargetRPCInfo.payload = value;
                p2pInfo.Instance.lastTargetRPCInfo.target = targetId;
                p2pInfo.Instance.lastTargetRPCInfo.isLargePacket = false;
            }
            SendPacketOptions options = new SendPacketOptions(){
                LocalUserId = EOSManager.Instance.GetProductUserId(),
                RemoteUserId = targetId.AsEpic,
                SocketId = p2pConnectorForOtherAssembly.Instance.SocketId,
                Channel = ch,
                AllowDelayedDelivery = p2pConfig.Instance.AllowDelayedDelivery,
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
        /// Send a packet to a specific peer.
        /// </summary>
        /// <param name="ch">(byte)ConnectHub.CHANNELLIST</param>
        /// <param name="value">Payload</param>
        /// <param name="targetId">Target to send</param>
        public static void SendPacket(byte ch, ArraySegment<byte> value, UserId targetId){
            SendPacketOptions options = new SendPacketOptions(){
                LocalUserId = EOSManager.Instance.GetProductUserId(),
                RemoteUserId = targetId.AsEpic,
                SocketId = p2pConnectorForOtherAssembly.Instance.SocketId,
                Channel = ch,
                AllowDelayedDelivery = p2pConfig.Instance.AllowDelayedDelivery,
                Reliability = p2pConfig.Instance.packetReliability,
                Data = value
            };

            ResultE result = p2pConnectorForOtherAssembly.Instance.P2PHandle.SendPacket(ref options);

            if(result != ResultE.Success){
                Debug.LogErrorFormat("Send Packet: can't send packet, code: {0}", result);
                return;
            }
        }
    #endregion
    #region LargePacket
        /// <summary>
        /// Send TargetRPC as LargePacket. <br />
        /// Max packet size is 296960byte.(1160byte * 256)
        /// </summary>
        /// <param name="ch"></param>
        /// <param name="value"></param>
        /// <param name="targetId"></param>
        public async static UniTask SendLargePackets(byte ch, byte[] value, UserId targetId){
            int length = 1160;
            byte[] header = GenerateHeader(value.Length);

        #if SYNICSUGAR_LOG
            Debug.Log($"SendLargePackets: PacketInfo:: size {value.Length} / chunk {header[1]}");
        #endif

            //Max payload is 1170 but we need some header.
            for(int startIndex = 0; startIndex < value.Length; startIndex += 1160){
                length = startIndex + 1160 < value.Length ? 1160 : value.Length - startIndex;
                SendPacket();
                //For sending buffer and main thread fps.
                if(header[0] % p2pConfig.Instance.LargePacketBatchSize == 0){
                    await UniTask.Yield();
                }

                //To use Span. However, this process generates Garbage by each loop.
                void SendPacket(){
                    Span<byte> _payload = new Span<byte>(value, startIndex, length); 
                    //Add header
                    Span<byte> payload = new byte[header.Length + length];
                    header.CopyTo(payload);
                    _payload.CopyTo(payload.Slice(2));

                    SendPacketOptions options = new SendPacketOptions(){
                        LocalUserId = EOSManager.Instance.GetProductUserId(),
                        RemoteUserId = targetId.AsEpic,
                        SocketId = p2pConnectorForOtherAssembly.Instance.SocketId,
                        Channel = ch,
                        AllowDelayedDelivery = true,
                        Reliability = p2pConfig.Instance.packetReliability,
                        Data = new ArraySegment<byte>(payload.ToArray())
                    };

                    ResultE result = p2pConnectorForOtherAssembly.Instance.P2PHandle.SendPacket(ref options);

                    if(result != ResultE.Success){
                        Debug.LogErrorFormat("Send Large Packet: can't send packet, code: {0}", result);
                        return;
                    }
                    //add index
                    header[0]++;
                }
            }
        #if SYNICSUGAR_LOG
            Debug.Log($"SendLargePackets: Finish to Send to {targetId}!");
        #endif
            /// <summary>
            /// index, chunk, sycned phase, is Specific sync, self or not
            /// </summary>
            byte[] GenerateHeader(int valueLength){
                byte[] result = new byte[2];

                result[0] = 0; 
                result[1] = (byte)Math.Ceiling(valueLength / 1100f);

                return result;
            }
        }
        
        /// <summary>
        /// Send TargetRPC as LargePacket. <br />
        /// Max packet size is 296960byte.(1160byte * 256)
        /// </summary>
        /// <param name="ch"></param>
        /// <param name="value"></param>
        /// <param name="targetId"></param>
        public async static UniTask SendLargePackets(byte ch, byte[] value, UserId targetId, bool recordPacketInfo){
            if(recordPacketInfo){
                p2pInfo.Instance.lastTargetRPCInfo.ch = ch;
                p2pInfo.Instance.lastTargetRPCInfo.payload = value;
                p2pInfo.Instance.lastTargetRPCInfo.target = targetId;
                p2pInfo.Instance.lastTargetRPCInfo.isLargePacket = true;
            }

            int length = 1160;
            byte[] header = GenerateHeader(value.Length);

        #if SYNICSUGAR_LOG
            Debug.Log($"SendLargePackets: PacketInfo:: size {value.Length} / chunk {header[1]}");
        #endif

            //Max payload is 1170 but we need some header.
            for(int startIndex = 0; startIndex < value.Length; startIndex += 1160){
                length = startIndex + 1160 < value.Length ? 1160 : value.Length - startIndex;
                SendPacket();
                //For sending buffer and main thread fps.
                if(header[0] % p2pConfig.Instance.LargePacketBatchSize == 0){
                    await UniTask.Yield();
                }

                //To use Span. However, this process generates Garbage by each loop.
                void SendPacket(){
                    Span<byte> _payload = new Span<byte>(value, startIndex, length); 
                    //Add header
                    Span<byte> payload = new byte[header.Length + length];
                    header.CopyTo(payload);
                    _payload.CopyTo(payload.Slice(2));

                    SendPacketOptions options = new SendPacketOptions(){
                        LocalUserId = EOSManager.Instance.GetProductUserId(),
                        RemoteUserId = targetId.AsEpic,
                        SocketId = p2pConnectorForOtherAssembly.Instance.SocketId,
                        Channel = ch,
                        AllowDelayedDelivery = true,
                        Reliability = p2pConfig.Instance.packetReliability,
                        Data = new ArraySegment<byte>(payload.ToArray())
                    };

                    ResultE result = p2pConnectorForOtherAssembly.Instance.P2PHandle.SendPacket(ref options);

                    if(result != ResultE.Success){
                        Debug.LogErrorFormat("Send Large Packet: can't send packet, code: {0}", result);
                        return;
                    }
                    //add index
                    header[0]++;
                }
            }
        #if SYNICSUGAR_LOG
            Debug.Log($"SendLargePackets: Finish to Send to {targetId}!");
        #endif
            /// <summary>
            /// index, chunk, sycned phase, is Specific sync, self or not
            /// </summary>
            byte[] GenerateHeader(int valueLength){
                byte[] result = new byte[2];

                result[0] = 0; 
                result[1] = (byte)Math.Ceiling(valueLength / 1100f);

                return result;
            }
        }
        /// <summary>
        /// Send RPC as LargePacket. <br />
        /// Max packet size is 296960byte.(1160byte * 256)
        /// </summary>
        /// <param name="ch"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public async static UniTask SendLargePacketsToAll(byte ch, byte[] value){
            foreach(var id in p2pInfo.Instance.userIds.RemoteUserIds){
                await SendLargePackets(ch, value, id);
            }
        #if SYNICSUGAR_LOG
            Debug.Log($"Finish SendLargePacketsToAll for {ch}");
        #endif
        }
        
        /// <summary>
        /// Send RPC as LargePacket. <br />
        /// Max packet size is 296960byte.(1160byte * 256)
        /// </summary>
        /// <param name="ch"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public async static UniTask SendLargePacketsToAll(byte ch, byte[] value, bool recordPacketInfo){
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
        /// <param name="syncedPhase">Sync from 0 to hierarchy</param>
        /// <param name="syncSpecificPhase">If false, synchronize an only specific hierarchy</param>
        /// <param name="isSelfData">Is it own data?</param>
        public static void SendSynicPackets(byte ch, byte[] value, UserId targetId, byte syncedPhase = 9, bool syncSpecificPhase = false, bool isSelfData = true){
            int length = 1100;
            byte[] header = GenerateHeader(value.Length, syncedPhase, syncSpecificPhase, isSelfData);

        #if SYNICSUGAR_LOG
            Debug.Log($"SendSynicPackets: PacketInfo:: size {value.Length} / chunk {header[1]} / hierarchy {header[2]} / syncSpecificPhase {header[3]}");
        #endif

            //Max payload is 1170 but we need some header.
            for(int startIndex = 0; startIndex < value.Length; startIndex += 1100){
                length = startIndex + 1100 < value.Length ? 1100 : value.Length - startIndex;

                Span<byte> _payload = new Span<byte>(value, startIndex, length); 
                //Add header
                Span<byte> payload = new byte[header.Length + length];
                header.CopyTo(payload);
                _payload.CopyTo(payload.Slice(5));

                SendPacketOptions options = new SendPacketOptions(){
                    LocalUserId = EOSManager.Instance.GetProductUserId(),
                    RemoteUserId = targetId.AsEpic,
                    SocketId = p2pConnectorForOtherAssembly.Instance.SocketId,
                    Channel = ch,
                    AllowDelayedDelivery = true,
                    Reliability = p2pConfig.Instance.packetReliability,
                    Data = new ArraySegment<byte>(payload.ToArray())
                };

                ResultE result = p2pConnectorForOtherAssembly.Instance.P2PHandle.SendPacket(ref options);

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
            /// <summary>
            /// index, chunk, sycned phase, is Specific sync, self or not
            /// </summary>
            byte[] GenerateHeader(int valueLength, byte phase, bool isOnly, bool isSelfData){
                byte[] result = new byte[5];

                result[0] = 0; 
                result[1] = (byte)Math.Ceiling(valueLength / 1100f);
                result[2] = phase;
                result[3] = isOnly ? (byte)1 : (byte)0;
                result[4] = isSelfData ? (byte)1 : (byte)0;

                return result;
            }
        }
    #endregion

    #region OBSOLETE
        public static void SendLargePacket(byte ch, byte[] value, UserId targetId, byte syncedPhase = 9, bool syncSpecificPhase = false, bool isSelfData = true){
            int length = 1100;
            byte[] header = GenerateHeader(value.Length, syncedPhase, syncSpecificPhase, isSelfData);

        #if SYNICSUGAR_LOG
            Debug.Log($"SendLargePacket: PacketInfo:: size {value.Length} / chunk {header[1]} / hierarchy {header[2]} / syncSpecificPhase {header[3]}");
        #endif

            //Max payload is 1170 but we need some header.
            for(int startIndex = 0; startIndex < value.Length; startIndex += 1100){
                length = startIndex + 1100 < value.Length ? 1100 : value.Length - startIndex;

                Span<byte> _payload = new Span<byte>(value, startIndex, length); 
                //Add header
                Span<byte> payload = new byte[header.Length + length];
                header.CopyTo(payload);
                _payload.CopyTo(payload.Slice(5));

                SendPacketOptions options = new SendPacketOptions(){
                    LocalUserId = EOSManager.Instance.GetProductUserId(),
                    RemoteUserId = targetId.AsEpic,
                    SocketId = p2pConnectorForOtherAssembly.Instance.SocketId,
                    Channel = ch,
                    AllowDelayedDelivery = true,
                    Reliability = p2pConfig.Instance.packetReliability,
                    Data = new ArraySegment<byte>(payload.ToArray())
                };

                ResultE result = p2pConnectorForOtherAssembly.Instance.P2PHandle.SendPacket(ref options);

                if(result != ResultE.Success){
                    Debug.LogErrorFormat("Send Large Packet: can't send packet, code: {0}", result);
                    return;
                }
                //add index
                header[0]++;
            }
        #if SYNICSUGAR_LOG
            Debug.Log($"Send Large Packet: Success to {targetId.ToString()}!");
        #endif
            /// <summary>
            /// index, chunk, sycned phase, is Specific sync, self or not
            /// </summary>
            byte[] GenerateHeader(int valueLength, byte phase, bool isOnly, bool isSelfData){
                byte[] result = new byte[5];

                result[0] = 0; 
                result[1] = (byte)Math.Ceiling(valueLength / 1100f);;
                result[2] = phase;
                result[3] = isOnly ? (byte)1 : (byte)0;
                result[4] = isSelfData ? (byte)1 : (byte)0;

                return result;
            }
        }
    #endregion
    }
}
