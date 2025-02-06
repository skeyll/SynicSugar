+++
title = "StopVoiceSending"
weight = 7
+++
## StopVoiceSending
<small>*Namespace: SynicSugar.RTC*</small>

public async UniTask&lt;Result&gt; StopVoiceSending()


### Description
Stop local user sending voice chat.<br>
If UseOpenVC is true, toggles setting state to send vc. If false, a thread for PushToTalk will stop.<br>


```cs
using SynicSugar.RTC;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class VCSample : MonoBehaviour {
    public async UniTask StopVC(){
       await RTCManager.Instance.StopVoiceSending();
    }
}
```