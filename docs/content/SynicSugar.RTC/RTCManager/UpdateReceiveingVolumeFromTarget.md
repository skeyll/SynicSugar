+++
title = "UpdateReceiveingVolumeFromTarget"
weight = 10
+++
## UpdateReceiveingVolumeFromTarget
<small>*Namespace: SynicSugar.RTC*</small>

public async UniTask&lt;Result&gt; UpdateReceiveingVolumeFromTarget(UserId targetId, float volume)


### Description
Change the output volume on this session.<br>
If targetId is null, effect to all remote users.<br>
Range 0.0 - 100. 50 means that the audio volume is not modified its source value. 100 makes volume double.


```cs
using SynicSugar.RTC;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class VCSample : MonoBehaviour {
    public async UniTaskVoid Start(){
        //Mute
        await RTCManager.Instance.UpdateReceiveingVolumeFromTarget(null, 0f);
        //Double
        RTCManager.Instance.UpdateReceiveingVolumeFromTarget(null, 100f).Forget();
    }
}
```