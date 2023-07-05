+++
title = "Attributes"
weight = 2
+++
## Attributes
<small>*Namespace: SynicSugar.MatchMake* <br>
*Class: Lobby* </small>

public List<LobbyAttribute> Attributes 


### Description
Attributes associated with the lobby.


```cs
using SynicSugar.MatchMake;

public class LobbyCondition : MonoBehaviour {
    void SetLobbyConditions(){
        Lobby lobby = new Lobby();

        LobbyAttribute attribute = new LobbyAttribute();

        attribute.Key = "Level";
        attribute.SetValue(3);
        attribute.comparisonOption = Epic.OnlineServices.ComparisonOp.Equal;
        lobby.Attributes.Add(attribute);
    }
}
```