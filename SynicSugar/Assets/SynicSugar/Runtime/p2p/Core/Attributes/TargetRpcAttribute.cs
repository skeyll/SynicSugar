using System;
namespace SynicSugar.P2P {
    [AttributeUsage(AttributeTargets.Method,
    Inherited = false)]
    public sealed class TargetRpcAttribute : Attribute {
        public bool isLargePacket;
        public bool recordLastPacket;
        /// <summary>
        /// For NetworkPlayer.
        /// </summary>
        public TargetRpcAttribute(){}
        
        /// <summary>
        /// Set options for sending packets to resend and the way to send.
        /// </summary>
        /// <param name="isLargePacket">If true, this process is sent as LargePacket.</param>
        /// <param name="shouldRecordLastPacketInfo">If true, the packet info is hold for manual resend.</param>
        public TargetRpcAttribute(bool isLargePacket, bool shouldRecordLastPacketInfo){
            this.isLargePacket = isLargePacket;
            recordLastPacket = shouldRecordLastPacketInfo;
        }
    }
}