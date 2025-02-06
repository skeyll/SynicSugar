+++
title = "StartVoiceSending"
weight = 6
+++
## StartVoiceSending
<small>*Namespace: SynicSugar.RTC*</small>

public async UniTask&lt;Result&gt; StartVoiceSending()


### Description
Starts local user sending voice chat.<br>
**Can call this after joining the lobby. However, should use this after get true from Matchmaking APIs. If we use VC just after Lobby participation, start receiving VC manually.**<br>
If UseOpenVC is true, toggles setting state to send vc. If false, a thread for PushToTalk will start.<br>
The receiving starts on StartPacketReceiver().


```cs
using SynicSugar.P2P;
using SynicSugar.RTC;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class VCSample : MonoBehaviour {
    public void StartVC(){
        RTCManager.Instance.StartVoiceSending().Forget();
    }
}
```