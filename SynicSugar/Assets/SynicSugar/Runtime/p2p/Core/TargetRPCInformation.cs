namespace SynicSugar.P2P {
    public class TargetRPCInformation {
        internal byte[] payload;
        internal byte ch;
        internal UserId target;
        internal bool isLargePacket;
    }
}