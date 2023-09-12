+++
title = "GetTargetAttributeData"
weight = 25
+++
## GetTargetAttributeData
<small>*Namespace: SynicSugar.MatchMake* <br>
*Class: MatchMakeManager* </small>

public List&lt;AttributeData&gt; GetTargetAttributeData(UserId target)<br>
public AttributeData GetTargetAttributeData(UserId target, string Key)


### Description
Get target's attribute(s) in the current lobby.<br>
**These information is basically for matchmaking. About data for in-game, we should exchange it via P2P for the security and server-bandwidth.**


```cs
using System.Collections.Generic;
using SynicSugar.MatchMake;
using SynicSugar.P2P;
using UnityEngine;

public class MatchMakeSample : MonoBehaviour {
    public void GetMemberAttribute(UserId target){
        List<AttributeData> data = MatchMakeManager.Instance.GetTargetAttributeData(target);
        
        Debug.Log(data[0].Value);
    }
}
```