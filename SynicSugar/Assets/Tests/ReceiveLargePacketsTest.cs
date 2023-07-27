using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using MemoryPack;
using MemoryPack.Compression;

namespace SynicSugar.P2P{
    public class ReceiveLargePacketsTest : MonoBehaviour {
        // Start is called before the first frame update
        enum CHANNELLIST{
            Test, None
        }
        /// <summary>
        /// Sync all Synic variables. This is very heavy because it handles multiple data and repeats compression and serialization.
        /// </summary>
        /// <param name="targetId">Target to be synced by this local user.</param>
        /// <param name="syncedHierarchy">Hierarchy to be synced. If syncSingleHierarchy is false, sync all variables in the hierarchy up to this point.</param>
        /// <param name="syncSingleHierarchy">If true, send only variables in syncedHierarchy.</param>
        public static void SyncSynic(UserId targetId, byte syncedHierarchy = 9, bool syncSingleHierarchy = false) {
            SynicContainer synicContainer = new SynicContainer();
            switch(syncedHierarchy){
                case 0:
                    // SynicItem0 synicItem0 = new SynicItem0{ noSyncData = 3};
                    // synicContainer.SynicItem0 = JsonUtility.ToJson(synicItem0);
                    break;
                case 1:
                    // DebugInt(1);
                    if(syncSingleHierarchy){ break; }
                    else{ goto case 0; }
                case 2:
                    // DebugInt(2);
                    if(syncSingleHierarchy){ break; }
                    else{ goto case 1; }
                case 3:
                    // DebugInt(3);  
                    if(syncSingleHierarchy){ break; }
                    else{ goto case 2; }
                default:
                    goto case 3;
            }

            using var compressor  = new BrotliCompressor();
            MemoryPackSerializer.Serialize(compressor , synicContainer);

            EOSp2p.SendLargePacket(255, compressor.ToArray(), targetId, syncedHierarchy, syncSingleHierarchy);
        }
        public void ConvertFormPacket(SugarPacket packet){
            switch((CHANNELLIST)packet.ch){
                case CHANNELLIST.Test:
                break;
                case CHANNELLIST.None:
                bool restoredPacket = RestorePackets(packet);

                if(!restoredPacket){
    #if SYNICSUGAR_LOG 
                    Debug.LogFormat("ConvertFormPacket: Restore packet is in progress. From {0}", packet.UserID);
    #endif
                    break;
                }
                // 受信処理(LargePacketInfomation)
                //Data二つを=nullして開放
                break;
            }
        }
        // Dictionary<string, List<ArraySegment<byte>>> buffer = new Dictionary<string, List<ArraySegment<byte>>>();
        // Dictionary<string, LargePacketInfomation> packetInfo = new Dictionary<string, LargePacketInfomation>();
        // void SyncedSynics(UserId targetId, byte[] restoredPacket){
        //     //Deserialize packet
        //     using var decompressor = new BrotliDecompressor();
        //     var decompressedBuffer = decompressor.Decompress(restoredPacket);
        //     SynicContainer container = MemoryPackSerializer.Deserialize<SynicContainer>(decompressedBuffer);

        //     //Packet data
        //     int hierarchy = packetInfo[targetId.ToString()].hierarchy;
        //     bool syncSingleHierarchy = packetInfo[targetId.ToString()].syncSpecificHierarchy;

        //     switch(hierarchy){
        //         case 0:
        //             SynicItem0 synicItem0 = JsonUtility.FromJson<SynicItem0>(container.SynicItem0);
        //             //noSyncData = synicItem0.noSyncData;
        //             break;
        //         case 1:
        //             // DebugInt(1);
        //             if(syncSingleHierarchy){ break; }
        //             else{ goto case 0; }
        //         case 2:
        //             // DebugInt(2);
        //             if(syncSingleHierarchy){ break; }
        //             else{ goto case 1; }
        //         case 3:
        //             // DebugInt(3);  
        //             if(syncSingleHierarchy){ break; }
        //             else{ goto case 2; }
        //         default:
        //             goto case 3;
        //     }
        // }
        
        Dictionary<string, byte[]> buffer = new Dictionary<string, byte[]>();
        Dictionary<string, LargePacketInfomation> packetInfo = new Dictionary<string, LargePacketInfomation>();
        bool RestorePackets(SugarPacket packet){
            if(!buffer.ContainsKey(packet.UserID)){
                packetInfo.Add(packet.UserID, new LargePacketInfomation(){  chunk = packet.payload[1], 
                                                                            hierarchy = packet.payload[2], 
                                                                            syncSpecificHierarchy = packet.payload[3] == 1 ? true : false });
                //Prep enough byte[]
                buffer.Add(packet.UserID, new byte[packet.payload[1] * 1100]);
            }
            int packetIndex = packet.payload[0];
            int offset = packetIndex * 1100;

            Span<byte> packetPayload = packet.payload.Slice(4);
            //Copy Byte from what come in
            Buffer.BlockCopy(packetPayload.ToArray(), 0, buffer[packet.UserID], offset, packetPayload.Length);
            packetInfo[packet.UserID].chunk --;
            //Comming all?
            return packetInfo[packet.UserID].chunk == 0 ? true : false;
        }

            // LargePacketInfomation これにIntCountかなにかの項目を追加して

            //Hold packet in order
            // if(packetIndex == buffer[packet.UserID].Count - 1){ //same with length 
            //     buffer[packet.UserID].Add(packet.payload);
            // }else if(packetIndex >= buffer[packet.UserID].Count){
            //     for(int length = buffer[packet.UserID].Count; length <= packetIndex; length++){ //short
            //         buffer[packet.UserID].Add(new ArraySegment<byte>());
            //     }
            // }else{ //enough
            //     buffer[packet.UserID][packetIndex] = packet.payload;
            // }

            // //Restore packet
            // if(buffer[packet.UserID].Count == packetInfo[packet.UserID].chunk){
            //     int length = buffer[packet.UserID].Sum(segment => segment.Count);
            //     byte[] result = new byte[length];
            //     int offset = 0;

            //     foreach (var i in buffer[packet.UserID]){
            //         i.Array.CopyTo(result, offset);
            //         offset += i.Count;
            //     }
            //     return result;
            // }
            // return null;

            // void RemoveLargePacketHeader(ref ArraySegment<byte> payload, bool isFirst){
            //     if(isFirst){
            //         payload = payload.Slice(4);
            //     }else{
            //         payload = payload.Slice(1);
            //     }
            // }
    }
}
