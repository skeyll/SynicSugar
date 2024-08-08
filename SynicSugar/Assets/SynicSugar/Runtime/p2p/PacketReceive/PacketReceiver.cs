using System;
using UnityEngine;
using Epic.OnlineServices;
namespace SynicSugar.P2P{
    public abstract class PacketReceiver : MonoBehaviour
    {
        protected byte ch_r;
        protected ProductUserId id_r;
        protected ArraySegment<byte> payload_r;
        protected byte maxBatchSize;
        protected IPacketConvert hub;
        /// <summary>
        /// Must manage this object active from here.
        /// </summary>
        /// <param name="ReceivingBatchSize"></param>
        public virtual void StartPacketReceiver(IPacketConvert hubInstance, byte ReceivingBatchSize = 1){
            hub = hubInstance;
            maxBatchSize = ReceivingBatchSize;
            this.enabled = true;
        }
        public virtual void  StopPacketReceiver(){
            this.enabled = false;
            maxBatchSize = 0;
        }
    }
}