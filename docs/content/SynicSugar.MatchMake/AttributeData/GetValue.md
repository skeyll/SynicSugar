+++
title = "GetValue"
weight = 1
+++
## GetValue
<small>*Namespace: SynicSugar.MatchMake* <br>
*Class: AttributeData* </small>

public static string GetValue(this AttributeData data)<br>
public static string GetValue(this List&lt;AttributeData&gt; list, string Key)<br>

### Description
Get specific value from user attributes or attribute.<br>
Type need be specified, and an exception will be thrown if the type of AttributeData and T are different.


```cs
using System.Collections.Generic;
using SynicSugar.MatchMake;
using SynicSugar.P2P;
using UnityEngine;

public class LobbyCondition : MonoBehaviour {
    public static List<AttributeData> GenerateUserAttribute(){
        //We can set max 100 attributes.
        List<AttributeData> attributeData = new();
        //Name
        AttributeData name = new (){
            Key = "NAME"
        };
        name.SetValue("USER NAME"); //string
        attributeData.Add(name); 

        Debug.Log(name.GetValue<string>("NAME")); // USER NAME

        //Charcter Type
        AttributeData c_type = new ();
        c_type.Key = "TYPE";
        c_type.SetValue(31); //int

        attributeData.Add(c_type);
        // Debug.Log(name.GetValue<string>("TYPE")); Error
        Debug.Log(name.GetValueAsString("TYPE")); // 31 as string
        Debug.Log(name.GetValue<int>("TYPE")); // 31 as int

        return attributeData;
    }

    void OnUpdatedMemberAttribute(UserId target){
        List<AttributeData> data = MatchMakeManager.Instance.GetTargetAttributeData(target);
        // data.GetValue<string>("TYPE") is Error.
        // To use this value on GUI, we can use AttributeData.GetValueAsString(data, "LEVEL")

        Debug.LogFormat("NAME: {0} / TYPE: {1}", data.GetValue<string>("NAME"), data.GetValue<int>("TYPE"));
    }
}
```