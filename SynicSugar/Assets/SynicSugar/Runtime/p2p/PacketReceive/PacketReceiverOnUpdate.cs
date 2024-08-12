namespace SynicSugar.P2P {
    public class PacketReceiverOnUpdate : PacketReceiver {
        void Update(){
            for(int i = 0; i < maxBatchSize; i++){
                bool recivePacket = getPacket.GetPacketFromBuffer(ref ch_r, ref id_r, ref payload_r);
                //Skip to next frame.
                if(!recivePacket){
                    break;
                }

                hub.ConvertFromPacket(ref ch_r, UserId.GetUserId(id_r).ToString(), ref payload_r);
            }
        }
    }
}