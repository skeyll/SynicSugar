+++
title = "MemberUpdatedNotifier"
weight = 20
+++
## MemberUpdatedNotifier
<small>*Namespace: SynicSugar.MatchMake* <br>
*Class: MatchMakeManager* </small>

public MemberUpdatedNotifier MemberUpdatedNotifier


### Description
Invoke when a user attributes is updated in current lobby.<br>
This attributes usually is used for manual matchmaking. Host kicks a member from the lobby with this information. So, we need to display these on GUI in this event.


### Event
| API | description |
|---|---|
| OnAttributesUpdated(UserId target) | Invoke when a user attributes is updated in current lobby |

### Function
| API | description |
|---|---|
| Register | Set events |


```cs
using System.Collections.Generic;
using SynicSugar.MatchMake;
using UnityEngine;

public class MatchMakeSample {
    void Start(){
        MatchMakeManager.Instance.MemberUpdatedNotifier.OnAttributesUpdated += t => OnUpdatedMemberAttribute(t);
    }
    
    void OnUpdatedMemberAttribute(UserId target){
        List<AttributeData> data = MatchMakeManager.Instance.GetTargetAttributeData(target);

        foreach(var attribute in data){
            Debug.Log($"{attribute.Key}: {attribute.GetValueAsString()}");
        }
    }
}
```