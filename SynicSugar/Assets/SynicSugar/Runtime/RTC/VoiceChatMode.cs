namespace SynicSugar.RTC {
    public enum VoiceChatMode 
    {
        /// <summary>
        /// Automatically transmits voice when detected. Transmission stops when no voice is detected.
        /// </summary>
        VoiceActivated, 
        /// <summary>
        /// Transmits voice only while the specified key is held down. Transmission stops when the key is released.
        /// </summary>
        PushToTalk
    }
}