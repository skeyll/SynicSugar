+++
title = "VoiceChatMode"
weight = 1
+++
## VoiceChatMode
<small>*Namespace: SynicSugar.RTC*</small>

public VoiceChatMode VoiceChatMode;

```cs
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
```

### Description
Switch OpenVC and PushToTalk.<br>
**This is valid only before matching.** If we want to switch that after matching, call ToggleLocalUserSending() by ourself.

```cs
using SynicSugar.RTC;
using UnityEngine;

public class VCSample : MonoBehaviour {
    void Start(){
        //Push to Talk
        RTCManager.Instance.VoiceChatMode = VoiceChatMode.PushToTalk;
    }
}
```