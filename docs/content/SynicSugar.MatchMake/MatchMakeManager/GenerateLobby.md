+++
title = "GenerateLobby"
weight = 10
+++
## GenerateLobby
public static Lobby GenerateLobby(string mode = "", string region = "",
                                            string mapName = "", uint MaxPlayers = 2,
                                            bool bPresenceEnabled = false)

### Description
Create a lobby for search and hosting conditions in local. Generate by large conditions like as BucketID and lobby capacity, then set individual conditions with LobbyAttribute.
LobbyAttribute needs Key, Value, and ComparisonOption. Key is string. Value is bool, int, double, and string. ComparisonOption's detail is [here](https://dev.epicgames.com/docs/en-US/game-services/lobbies#comparison-operators).
*This funcition is just for **conditions**.

```cs
using SynicSugar.MatchMake;

public class MatchMake : MonoBehaviour {
    void Lobby GetLobbyCondition(){
        Lobby lobbyCondition = MatchMakeManager.GenerateLobby("Rank", "ASIA");
        
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