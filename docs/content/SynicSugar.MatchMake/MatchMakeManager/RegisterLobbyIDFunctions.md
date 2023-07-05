+++
title = "RegisterLobbyIDFunctions"
weight = 8
+++
## RegisterLobbyIDFunctions
<small>*Namespace: SynicSugar.MatchMake* <br>
*Class: MatchMakeManager* </small>

public void RegisterLobbyIDFunctions(Action save, Action delete, bool changeType = true)


### Description 
Register functions to save and delete LobbyId to re-connect on finishing Matchmaking.<br>
We can use cloud and save assets for this, but these place to be saved and deleted must be in the same. 

If changeType is true, lobbyIdSaveType becomes CustomMethod.


```cs
using SynicSugar.MatchMake;

public class MatchMake : MonoBehaviour {
    void Start(){
        MatchMakeManager.Instance.RegisterLobbyIDFunctions((() => SaveFunction()), ((() =>  DeleteFunction())), true);
    }

    public void SaveFunction(){

    }
    public void DeleteFunction(){
        
    }
}
```