namespace SynicSugar.P2P {
    public enum SessionType {
        /// <summary>
        /// Not in session.
        /// </summary>
        None,
        /// <summary>
        /// In session online with other users
        /// </summary>
        OnlineSession,
        /// <summary>
        /// In pseudo-session mode without network connection (single-player simulation).
        /// </summary>
        OfflineSession,
        /// <summary>        
        /// The lobby was closed by host or the local user disconnected from the lobby, causing the (p2p) session to end.
        /// </summary>
        InvalidSession
    }
}
