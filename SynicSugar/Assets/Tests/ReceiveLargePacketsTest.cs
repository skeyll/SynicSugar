using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace SynicSugar.P2P{
    public class ReceiveLargePacketsTest : MonoBehaviour {
        // Start is called before the first frame update
        enum CHANNELLIST{
            Test, None
        }
        public void ConvertFormPacket(SugarPacket packet){
            switch((CHANNELLIST)packet.ch){
                case CHANNELLIST.Test:
                break;
                case CHANNELLIST.None:
                byte[] result = RestorePackets(packet);

                if(result == null){
    #if SYNICSUGAR_LOG 
                    Debug.Log("ConvertFormPacket: large packet is not enough");
    #endif
                    break;
                }
                // 受信処理(LargePacketInfomation)
                //Data二つを=nullして開放
                break;
            }
        }
        Dictionary<string, List<ArraySegment<byte>>> buffer = new Dictionary<string, List<ArraySegment<byte>>>();
        Dictionary<string, LargePacketInfomation> packetInfo = new Dictionary<string, LargePacketInfomation>();
        byte[] RestorePackets(SugarPacket packet){
            if(!buffer.ContainsKey(packet.UserID)){
                buffer.Add(packet.UserID, new List<ArraySegment<byte>>());
                packetInfo.Add(packet.UserID, new LargePacketInfomation());
            }
            int packetIndex = packet.payload[0];
            bool isFirstPacket = packetIndex == 0;
            if(isFirstPacket){
                packetInfo[packet.UserID].chunk = packet.payload[1];
                packetInfo[packet.UserID].hierarchy = packet.payload[2];
                packetInfo[packet.UserID].syncSpecificHierarchy = packet.payload[3] == 1 ? true : false;
            }

            RemoveLargePacketHeader(ref packet.payload, isFirstPacket);

            //Hold packet in order
            if(packetIndex == buffer[packet.UserID].Count - 1){ //same with length 
                buffer[packet.UserID].Add(packet.payload);
            }else if(packetIndex >= buffer[packet.UserID].Count){
                for(int length = buffer[packet.UserID].Count; length <= packetIndex; length++){ //short
                    buffer[packet.UserID].Add(new ArraySegment<byte>());
                }
            }else{ //enough
                buffer[packet.UserID][packetIndex] = packet.payload;
            }

            //Restore packet
            if(buffer[packet.UserID].Count == packetInfo[packet.UserID].chunk){
                int length = buffer[packet.UserID].Sum(segment => segment.Count);
                byte[] result = new byte[length];
                int offset = 0;

                foreach (var i in buffer[packet.UserID]){
                    i.Array.CopyTo(result, offset);
                    offset += i.Count;
                }
                return result;
            }
            return null;
        }
        void RemoveLargePacketHeader(ref ArraySegment<byte> payload, bool isFirst){
            if(isFirst){
                payload = payload.Slice(4);
            }else{
                payload = payload.Slice(1);
            }
        }
    }
    public class LargePacketInfomation {
        public byte chunk;
        public byte hierarchy;
        public bool syncSpecificHierarchy;
    }
}
