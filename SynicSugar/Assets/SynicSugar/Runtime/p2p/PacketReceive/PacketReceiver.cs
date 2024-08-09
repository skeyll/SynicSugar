using System;
using UnityEngine;
using Epic.OnlineServices;
namespace SynicSugar.P2P{
    public abstract class PacketReceiver : MonoBehaviour
    {
        protected byte ch_r;
        protected ProductUserId id_r;
        protected ArraySegment<byte> payload_r;
        protected uint maxBatchSize;
        protected IPacketConvert hub;
        protected IGetPacket getPacket;
        void Awake(){
            enabled = false;
        }
        public void SetGetPacket(IGetPacket instance){
            getPacket = instance;
        }
        /// <summary>
        /// Must manage this object active from here.
        /// </summary>
        /// <param name="ReceivingBatchSize"></param>
        public virtual void StartPacketReceiver(IPacketConvert hubInstance, uint ReceivingBatchSize = 1){
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