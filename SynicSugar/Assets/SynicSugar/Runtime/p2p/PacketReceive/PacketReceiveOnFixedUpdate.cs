using System;
using UnityEngine;
namespace SynicSugar.P2P {
    public class PacketReceiveOnFixedUpdate : MonoBehaviour {
        byte ch_r;
        string id_r;
        ArraySegment<byte> payload_r;
        int maxBatchSize;
        IPacketReciver hub;
        /// <summary>
        /// Must manage this object active from here.
        /// </summary>
        /// <param name="ReceivingBatchSize"></param>
        internal void StartPacketReceiving(IPacketReciver hubInstance, int ReceivingBatchSize = 1){
            hub = hubInstance;
            maxBatchSize = ReceivingBatchSize;
            this.enabled = true;
        }
        internal void StopPacketReceiving(){
            this.enabled = false;
            maxBatchSize = 0;
        }
        void FixedUpdate(){
            for(int i = 0; i < maxBatchSize; i++){
                bool recivePacket = p2pConnectorForOtherAssembly.Instance.GetPacketFromBuffer(ref ch_r, ref id_r, ref payload_r);
                //Skip to next frame.
                if(!recivePacket){
                    break;
                }
                hub.ConvertFromPacket(ref ch_r, ref id_r, ref payload_r);
            }
        }
    }
}