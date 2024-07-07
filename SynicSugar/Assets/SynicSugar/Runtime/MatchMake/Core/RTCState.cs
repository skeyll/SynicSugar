namespace SynicSugar.MatchMake {
    /// <summary>
    /// The class to manage RTC state of each user in in-game.
    /// </summary>
    public class RTCState {
        public bool IsInRTCRoom { get; internal set; } = false;
        public bool IsSpeakinging { get; internal set; } = false;
        public bool IsAudioOutputEnabled { get; internal set; } = false;
        public bool IsHardMuted { get; internal set; } = false;
        public bool IsLocalMute { get; internal set; } = false;
        public float LocalOutputedVolume { get; internal set; } = 50.0f;
    }
}