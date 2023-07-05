+++
title = "Key"
weight = 0
+++
## Key
<small>*Namespace: SynicSugar.MatchMake* <br>
*Class: LobbyAttribute* </small>

public string Key 

### Description
The Key for this attribute.


```cs
using SynicSugar.MatchMake;

public class LobbyCondition : MonoBehaviour {
    void SetLobbyConditions(){
        LobbyAttribute attribute = new LobbyAttribute();
        attribute.Key = "Level";
    }
}
```