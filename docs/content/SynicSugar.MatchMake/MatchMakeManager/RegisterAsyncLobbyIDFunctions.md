+++
title = "RegisterAsyncLobbyIDFunctions"
weight = 9
+++
## RegisterAsyncLobbyIDFunctions
<small>*Namespace: SynicSugar.MatchMake* <br>
*Class: MatchMakeManager* </small>

public void RegisterAsyncLobbyIDFunctions(Func<UniTask> save, Func<UniTask> delete, bool changeType = true)


### Description 
Register functions to save and delete LobbyId to re-connect on finishing Matchmaking.<br>
We can use cloud and save assets for this, but these place to be saved and deleted must be in the same. 

If changeType is true, lobbyIdSaveType becomes AsyncCustomMethod.


```cs
using Cysharp.Threading.Tasks;
using SynicSugar.MatchMake;

public class MatchMake : MonoBehaviour {
    void Start(){
        MatchMakeManager.Instance.RegisterAsyncLobbyIDFunctions((async() => await AsyncSaveFunction()), ((async() => await AsyncDeleteFunction())), true);
    }

    public async UniTask AsyncSaveFunction(){

    }
    public async UniTask AsyncDeleteFunction(){
        
    }
}
```