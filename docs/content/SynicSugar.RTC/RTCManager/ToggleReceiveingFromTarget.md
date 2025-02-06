+++
title = "ToggleReceiveingFromTarget"
weight = 9
+++
## ToggleReceiveingFromTarget
<small>*Namespace: SynicSugar.RTC*</small>

public async UniTask&lt;Result&gt; ToggleReceiveingFromTarget(UserId targetId, bool isEnable)


### Description
Switch Output setting(Enable or Mute) of receiving from target user on this Session.<br>
If targetId is null, effect to all remote users.


```cs
using SynicSugar.RTC;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class VCSample : MonoBehaviour {
    public async UniTaskVoid Start(){
        await RTCManager.Instance.ToggleReceiveingFromTarget(null, true);
    }
}
```