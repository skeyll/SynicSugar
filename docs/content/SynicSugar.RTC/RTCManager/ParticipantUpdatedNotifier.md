+++
title = "ParticipantUpdatedNotifier"
weight = 0
+++
## ParticipantUpdatedNotifier
<small>*Namespace: SynicSugar.RTC*</small>

public ParticipantUpdatedNotifier ParticipantUpdatedNotifier


### Description
Events when an audio state is changed.<br>
We register events to change speaking or not on GUI.<br>


### Event
| API | description |
|---|---|
| OnStartSpeaking | Invoke on ParticipantUpdated |
| OnStopSpeaking | Invoke on ParticipantUpdated |
| OnStartTargetSpeaking | Invoke on ParticipantUpdated with UserId as arg |
| OnStopTargetSpeaking | Invoke on ParticipantUpdated with UserId as arg |


### Function
| API | description |
|---|---|
| Register | Set events |


```cs
using SynicSugar.P2P;
using SynicSugar.RTC;
using UnityEngine;

public class VCSample : MonoBehaviour {
    void Start(){
        RTCManager.Instance.ParticipantUpdatedNotifier.Register(() => OnStartSpeaking(), () => OnStopSpeaking());
    }
    void OnStartSpeaking(){
        Debug.Log($"{RTCManager.Instance.LastStateUpdatedUserId.ToString()} is Speaking");
    }
    void OnStopSpeaking(){
        Debug.Log($"{RTCManager.Instance.LastStateUpdatedUserId.ToString()} is Mute");
    }
}
```