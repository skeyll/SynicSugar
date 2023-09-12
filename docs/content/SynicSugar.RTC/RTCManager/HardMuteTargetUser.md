+++
title = "HardMuteTargetUser"
weight = 11
+++
## HardMuteTargetUser
<small>*Namespace: SynicSugar.RTC*</small>

public void HardMuteTargetUser(UserId target, bool isMuted)


### Description
Host mutes target user('s input) on all locals. The target can't speak but can hear other members of the lobby.


```cs
using SynicSugar.P2P;
using SynicSugar.RTC;
using UnityEngine;

public class VCSample : MonoBehaviour {
    public void BanTarget(UserId target){
        RTCManager.Instance.HardMuteTargetUser(target, true);
    }
}
```