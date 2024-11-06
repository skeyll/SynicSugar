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
        OfflineSession
    }
}
