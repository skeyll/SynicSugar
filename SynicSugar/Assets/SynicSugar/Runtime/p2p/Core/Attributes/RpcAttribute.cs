using System;
namespace SynicSugar.P2P {
    [AttributeUsage(AttributeTargets.Method,
    Inherited = false)]
    public sealed class RpcAttribute : Attribute {
        public bool isLargePacket;
        public bool recordLastPacket;
        /// <summary>
        /// For NetworkPlayer and NetoworkCommons. Just Send packet. 
        /// </summary>
        public RpcAttribute(){}
        /// <summary>
        /// Set options for sending packets to resend and the way to send.
        /// </summary>
        /// <param name="shouldRecordLastPacketInfo">If true, the packet info is hold for manual resend.</param>
        /// <param name="isLargePacket">If true, this process is sent as LargePacket.</param>
        public RpcAttribute(bool isLargePacket, bool shouldRecordLastPacketInfo){
            this.isLargePacket = isLargePacket;
            recordLastPacket = shouldRecordLastPacketInfo;
        }
    }
}