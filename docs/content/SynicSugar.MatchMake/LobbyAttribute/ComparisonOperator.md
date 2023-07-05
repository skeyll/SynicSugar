+++
title = "ComparisonOperator"
weight = 1
+++
## ComparisonOperator
<small>*Namespace: SynicSugar.MatchMake* <br>
*Class: LobbyAttribute* </small>

public Epic.OnlineServices.ComparisonOp ComparisonOperator

### Description
Comparison Operator for this attribute.<br>
Check more detail in [EOS document](https://dev.epicgames.com/docs/game-services/lobbies#comparison-operators).


```cs
using SynicSugar.MatchMake;

public class LobbyCondition : MonoBehaviour {
    void SetLobbyConditions(){
        LobbyAttribute attribute = new LobbyAttribute();
        attribute.comparisonOption = Epic.OnlineServices.ComparisonOp.Equal;
    }
}
```