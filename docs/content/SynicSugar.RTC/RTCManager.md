+++
title = "RTCManager"
weight = 1
+++

## RTCManager
<small>*Namespace: SynicSugar.RTC*</small>


### Description
Manage around RTC on session. <br>
This script is Mono's Singleton attached to NetworkManager. To generate NetworkManager, right-click on the Hierarchy and click SynicSugar/NetworkManager
NetworkManager has DontDestroy, so NetworkManager will not be destroyed by scene transitions. 


### Properity
| API | description |
|---|---|
| [ParticipantUpdatedNotifier](../RTCManager/participantupdatednotifier) | Notification whether a member is speaking |
| IsVoiceChatEnabled | Indicates whether the current Lobby has VC enabled |
| IsLocalSendingEnabled | Indicates whether local sending is enabled |
| IsLocalReceivingEnabled | Indicates whether local receiving is enabled |
| [VoiceChatMode](../RTCManager/voicechatmode) | Voice chat mode of this Manager |
| [KeyToPushToTalk](../RTCManager/keytopushtotalk) | the key or keycode for PushToTalk |
| LastStateUpdatedUserId | ID of the user whose status was last updated |

### Function
| API | description |
|---|---|
| [StartVoiceSending](../RTCManager/startvoicesending) | Starts local user sending voice chat |
| [StopVoiceSending](../RTCManager/stopvoicesending) | Stop local user sending voice chat |
| [ToggleLocalUserSending](../RTCManager/togglelocalusersending) | Switch Input setting of Local user sending |
| [ToggleReceiveingFromTarget](../RTCManager/togglereceiveingfromtarget) | Switch Output setting(Enable or Mute) of receiving from target |
| [UpdateReceiveingVolumeFromTarget](../RTCManager/updatereceiveingvolumefromtarget) | Change the receiving volume on this Session |
| [HardMuteTargetUser](../RTCManager/hardmutetargetuser) | Host mutes target user('s input) |
| [TargetOutputedVolumeOnLocal](../RTCManager/targetoutputedvolumeonlocal) | Outputed volume on this local of target |
| [TryGetTargetOutputedVolumeOnLocal](../RTCManager/targetoutputedvolumeonlocal) | Outputed volume on this local of target |
| [IsTargetMutedOnLocal](../RTCManager/istargetmutedonlocal) | Target state is mute or not  |
| [TryGetIsTargetMutedOnLocal](../RTCManager/istargetmutedonlocal) | Target state is mute or not  |
| [IsTargetHardMuted](../RTCManager/istargethardmuted) | Target state is hard mute or not |
| [TryGetIsTargetHardMuted](../RTCManager/istargethardmuted) | Target state is hard mute or not |


```cs
using SynicSugar.P2P;
using SynicSugar.RTC;
using UnityEngine;

public class NetworkSample : MonoBehaviour {
    void Start() {
        //Start ReceivingVC on StartPacketReceiver().

        RTCManager.Instance.StartVoiceSending();
    }
    public void BanTarget(UserId target){
        RTCManager.Instance.HardMuteTargetUser(target, true);
    }
}
```