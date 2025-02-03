namespace SynicSugar.RTC {
    public enum VCMode 
    {
        /// <summary>
        /// OpenVC toggles between on and off. When on, voice communication is automatically sent, and when off, the voice communication is stopped.
        /// </summary>
        OpenVC, 
        /// <summary>
        /// PushToTalk sends voice communication only while the specified key is held down. When the key is released, voice communication stops."
        /// </summary>
        PushToTalk
    }
}