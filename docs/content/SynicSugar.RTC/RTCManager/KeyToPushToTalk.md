+++
title = "KeyToPushToTalk"
weight = 2
+++
## KeyToPushToTalk
<small>*Namespace: SynicSugar.RTC*</small>

#if ENABLE_LEGACY_INPUT_MANAGER
public KeyCode KeyToPushToTalk = KeyCode.Space;
#else
public UnityEngine.InputSystem.Key KeyToPushToTalk = UnityEngine.InputSystem.Key.Space;
#endif


### Description
Key for PushToTalk.<br>
VC is sent only when user is pressing this button, and  when releases, automatically mutes.

```cs
using SynicSugar.RTC;
using UnityEngine;

public class VCSample : MonoBehaviour {
    void Start(){
        RTCManager.Instance.KeyToPushToTalk = KeyCode.T;
    }
}
```