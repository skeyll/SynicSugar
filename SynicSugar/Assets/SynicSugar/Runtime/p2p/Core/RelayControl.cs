namespace SynicSugar.P2P {
    /// <summary>
    /// Users with settings A and B cannot connect.
    /// So, we should use only AllowRelays and one of the other settings.
    /// </summary>
    public enum RelayControl {
        /// <summary>
        /// Peer connections will never attempt to use relay servers. Clients with restrictive NATs may not be able to connect to peers.
        /// </summary>
        NoRelays, 
        /// <summary>
        /// Peer connections will attempt to use relay servers, but only after direct connection attempts fail. This is the default value if not changed.
        /// </summary>
        AllowRelays, 
        /// <summary>
        /// Peer connections will only ever use relay servers. This will add latency to all connections, but will hide IP Addresses from peers.
        /// </summary>
        ForceRelays
    }
}