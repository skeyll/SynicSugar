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
        protected Base.SessionCore getPacket;
        void Awake(){
            enabled = false;
        }
        public void SetGetPacket(Base.SessionCore instance){
            getPacket = instance;
        }
        /// <summary>
        /// Must manage this object active from here.
        /// </summary>
        /// <param name="ReceivingBatchSize"></param>
        public virtual void StartPacketReceiver(IPacketConvert hubInstance, uint ReceivingBatchSize = 1){
            #if SYNICSUGAR_LOG
                Debug.Log($"StartPacketReceiver: Activated {this.GetType()}.");
            #endif
            hub = hubInstance;
            maxBatchSize = ReceivingBatchSize;
            this.enabled = true;
        }
        public virtual void StopPacketReceiver(){
            #if SYNICSUGAR_LOG
                Debug.Log($"StartPacketReceiver: Stop {this.GetType()}.");
            #endif
            this.enabled = false;
            maxBatchSize = 0;
        }
    }
}