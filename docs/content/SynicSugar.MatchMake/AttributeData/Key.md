+++
title = "Key"
weight = 0
+++
## Key
<small>*Namespace: SynicSugar.MatchMake* <br>
*Class: AttributeData* </small>

public string Key 

### Description
The Key for this attribute.


```cs
using SynicSugar.MatchMake;

public class LobbyCondition : MonoBehaviour {
    void SetLobbyConditions(){
        AttributeData attribute = new AttributeData();
        attribute.Key = "Level";
    }
}
```