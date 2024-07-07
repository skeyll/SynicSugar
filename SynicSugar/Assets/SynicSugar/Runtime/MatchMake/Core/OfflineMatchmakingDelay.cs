namespace SynicSugar.MatchMake {
    /// <summary>
    /// Delay ms for offlinematchmaking. <br />
    /// Can set 0 - n ms.
    /// </summary>
    public readonly struct OfflineMatchmakingDelay {
        public readonly uint StartMatchmakingDelay;
        public readonly uint WaitForOpponentsDelay;
        public readonly uint FinishMatchmakingDelay;
        public readonly uint ReadyForConnectionDelay;
        /// <summary>
        /// If set 0, return true without no delay. 
        /// </summary>
        /// <param name="StartMatchmakingDelay">ms</param>
        /// <param name="WaitForOpponentsDelay">ms</param>
        /// <param name="FinishMatchmakingDelay">ms</param>
        /// <param name="ReadyForConnectionDelay">ms</param>
        public OfflineMatchmakingDelay(uint StartMatchmakingDelay, uint WaitForOpponentsDelay, uint FinishMatchmakingDelay, uint ReadyForConnectionDelay){
            this.StartMatchmakingDelay = StartMatchmakingDelay;
            this.WaitForOpponentsDelay = WaitForOpponentsDelay;
            this.FinishMatchmakingDelay = FinishMatchmakingDelay;
            this.ReadyForConnectionDelay = ReadyForConnectionDelay;
        }
        public static OfflineMatchmakingDelay NoDelay => new OfflineMatchmakingDelay(0, 0, 0, 0);
    }
}