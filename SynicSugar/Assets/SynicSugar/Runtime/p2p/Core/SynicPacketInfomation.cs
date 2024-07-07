namespace SynicSugar.P2P {
    /// <summary>
    /// Just for Synic.
    /// </summary>
    public class SynicPacketInfomation {
        public LargePacketsInfomation basis = new();
        /// <summary>
        /// Phase specified in SyncSynic
        /// </summary>
        public byte phase;
        /// <summary>
        /// For just a one phase?
        /// </summary>
        public bool syncSinglePhase;
    }
}