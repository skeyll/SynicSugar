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
}