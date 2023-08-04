+++
title = "GenerateLobbyObject"
weight = 19
+++
## GenerateLobbyObject
<small>*Namespace: SynicSugar.MatchMake* <br>
*Class: MatchMakeManager* </small>

public static Lobby GenerateLobbyObject(string[] bucket, uint MaxPlayers = 2)


### Description
Create a Lobby object for search and hosting conditions in local. Generate Lobby by  this, then set individual conditions with LobbyAttribute.

args bucket is important condition like game-mode, region, map.<br>
player is 2-64.

LobbyAttribute needs Key, Value, and ComparisonOption. The Key is string. Value can be bool, int, double, and string. ComparisonOption's detail is *[EOS document](https://dev.epicgames.com/docs/en-US/game-services/lobbies#comparison-operators)*.


```cs
using SynicSugar.MatchMake;

public class MatchMakeCondition {
    Lobby GetLobbyCondition(){
        Lobby lobbyCondition = MatchMakeManager.GenerateLobbyObject(new string[3]{"RANK", "ASIA", "SEA"});
        
        LobbyAttribute attribute = new LobbyAttribute();
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