namespace SynicSugar {
    public sealed class SynicSugarState {
        /// <summary>
        /// After call Login methods and get Success by these, this value becomes true.
        /// </summary>
        /// <value></value>
        public bool IsLoggedIn { get; internal set; }

        /// <summary>
        /// While call matchmaking methods, this value becomes true.<br />
        /// After finish matchmaking and start p2p connection, this value returns false.
        /// </summary>
        /// <value></value>
        public bool IsMatchmaking { get; internal set; }

        /// <summary>
        /// After get Success by matchmaking APIs include for Offline, this value becomes true　until finish Session by ExitSession or CloseSession.
        /// </summary>
        /// <value></value>
        public bool IsInSession { get; internal set; }
    }
}