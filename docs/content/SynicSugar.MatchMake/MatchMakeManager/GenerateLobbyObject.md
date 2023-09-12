+++
title = "GenerateLobbyObject"
weight = 19
+++
## GenerateLobbyObject
<small>*Namespace: SynicSugar.MatchMake* <br>
*Class: MatchMakeManager* </small>

public static Lobby GenerateLobbyObject(string[] bucket, uint MaxPlayers = 2, bool useVoiceChat = false)


### Description
Create a Lobby object in local for search and hosting conditions. Generate Lobby Object, then set individual conditions as *[AttributeData](../attributedata/)*.

bucket is important condition like game-mode, region, map. **EOS searches lobby with bucket at first, so to use bucket improves search performance.**<br>
player is 2-64.<br>
If useVoiceChat is true, the lobby has RTC room. If false here, the session cannot use VC.

AttributeData needs Key, Value, and ComparisonOption. The Key is string. Value can be bool, int, double, and string. ComparisonOption's detail is *[EOS document](https://dev.epicgames.com/docs/en-US/game-services/lobbies#comparison-operators)*.
Can set max 100 attributes.


```cs
using SynicSugar.MatchMake;

public class MatchMakeCondition {
    Lobby GetLobbyCondition(){
        Lobby lobbyCondition = MatchMakeManager.GenerateLobbyObject(new string[3]{"RANK", "ASIA", "SEA"});
        
        AttributeData attribute = new AttributeData();
        //1
        attribute.Key = "Level";
        attribute.SetValue(3);
        attribute.comparisonOption = Epic.OnlineServices.ComparisonOp.Equal;
        lobbyCondition.Attributes.Add(attribute);
        //2
        attribute.Key = "RoomID";
        attribute.SetValue("ROOM12345");
        attribute.comparisonOption = Epic.OnlineServices.ComparisonOp.Equal;
        lobbyCondition.Attributes.Add(attribute);
        //...
        return lobbyCondition;
    }
}
```