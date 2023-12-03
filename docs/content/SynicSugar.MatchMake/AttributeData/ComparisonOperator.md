+++
title = "ComparisonOp"
weight = 1
+++
## ComparisonOp
<small>*Namespace: SynicSugar.MatchMake* <br>

public enum ComparisonOp

### Description
Comparison Operator for this attribute.<br>
Check more detail in [EOS document](https://dev.epicgames.com/docs/game-services/lobbies#comparison-operators).


```cs
using SynicSugar.MatchMake;

public class LobbyCondition : MonoBehaviour {
    void SetLobbyConditions(){
        AttributeData attribute = new AttributeData();
        attribute.ComparisonOperator = ComparisonOp.Equal;
    }
}
```