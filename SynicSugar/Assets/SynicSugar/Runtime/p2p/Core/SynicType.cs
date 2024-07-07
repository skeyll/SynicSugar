namespace SynicSugar.P2P{
    public enum SynicType {
        /// <summary>
        /// Only sender data
        /// </summary>
        OnlySelf,
        /// <summary>
        /// Sender data and (Host) TargetData
        /// </summary>
        WithTarget,
        /// <summary>
        /// Sender data, (Host) TargetData and (Host) Disconencted user Data
        /// </summary>
        WithOthers
    }
}