namespace SynicSugar.P2P {
    /// <summary>
    /// To reconstruction large packet. 
    /// </summary>
    public class LargePacketsInfomation {
        /// <summary>
        /// How many packets are sent in a packet?
        /// </summary>
        public byte additionalPacketsAmount;
        /// <summary>
        /// Current packet size received
        /// </summary>
        public int currentSize;
    }
#region OBSOLETE
    public class LargePacketInfomation {
        /// <summary>
        /// How many packets are sent in a packet?
        /// </summary>
        public byte chunk;
        /// <summary>
        /// Phase specified in SyncSynic
        /// </summary>
        public byte phase;
        /// <summary>
        /// For just a one phase?
        /// </summary>
        public bool syncSinglePhase;
        /// <summary>
        /// Current packet size received
        /// </summary>
        public int currentSize;
    }
#endregion
}