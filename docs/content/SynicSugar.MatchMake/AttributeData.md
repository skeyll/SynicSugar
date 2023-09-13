+++
title = "AttributeData"
weight = 2
+++

## AttributeData
<small>*Namespace: SynicSugar.MatchMake*</small>

### Description
Attributes associated with Lobby to serach and create lobby.<br>
Max 100 attributes can be set.

### Properity
| API | description |
|---|---|
| [Key](../AttributeData/key) | The Key for this attribute |
| BOOLEAN | The value when it was set as bool |
| INT64 | The value when it was set as INT64 |
| DOUBLE | The value when it was set as DOUBLE |
| STRING | The value when it was set as STRING |
| ValueType | The value type of holding |
| [ComparisonOperator](../AttributeData/comparisonoperator) | Comparison Operator for this attribute |



### Function 
| API | description |
|---|---|
| [SetValue](../AttributeData/setvalue) | Set a value to the attribute |
| GetValueAsString | Get value as string from attribute |


### Static Function 
| API | description |
|---|---|
| [GetValueAsString](../AttributeData/getvalueasstring) | Get specific key value as string from attribute list |


```cs
using System.Collections.Generic;
using SynicSugar.MatchMake;
using UnityEngine;

public class MatchMakeSample {
    public void AttributeDebug(){
        List<AttributeData> attributes = MatchMakeManager.Instance.GetTargetAttributeData(target);
        foreach(var a in attributes){
            //If this API is not used, we need to get a value from the same type variable with ValueType.
            Debug.LogFormat("Key: {0} / Value: {1}", a.Key, a.GetValueAsString());
        }
    }
}
```