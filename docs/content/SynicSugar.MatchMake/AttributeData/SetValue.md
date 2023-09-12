+++
title = "SetValue"
weight = 1
+++
## SetValue
<small>*Namespace: SynicSugar.MatchMake* <br>
*Class: AttributeData* </small>

public void SetValue(bool value)<br>
public void SetValue(int value)<br>
public void SetValue(double value)<br>
public void SetValue(string value)


### Description
Set a value to the attribute


```cs
using SynicSugar.MatchMake;

public class LobbyCondition : MonoBehaviour {
    void SetLobbyConditions(){
        AttributeData attribute = new AttributeData();
        attribute.SetValue(100);
    }
}
```