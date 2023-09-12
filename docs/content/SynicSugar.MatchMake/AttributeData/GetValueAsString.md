+++
title = "GetValueAsString"
weight = 1
+++
## GetValueAsString
<small>*Namespace: SynicSugar.MatchMake* <br>
*Class: AttributeData* </small>

public static string GetValueAsString(List&lt;AttributeData&gt; list, string Key))<br>

### Description
Get specific value from user attributes.


```cs
using System.Collections.Generic;
using SynicSugar.MatchMake;
using SynicSugar.P2P;
using UnityEngine;

public class LobbyCondition : MonoBehaviour {

    void OnUpdatedMemberAttribute(UserId target){
        List<AttributeData> data = MatchMakeManager.Instance.GetTargetAttributeData(target);
        
        Debug.LogFormat("NAME: {0} / LEVEL: {1}", AttributeData.GetValueAsString(data, "NAME"), AttributeData.GetValueAsString(data, "LEVEL"));
    }
}
```