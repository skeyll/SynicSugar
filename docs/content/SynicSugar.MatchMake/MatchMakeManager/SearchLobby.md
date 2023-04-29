+++
title = "SearchLobby"
weight = 6
+++
## SearchLobby
public async UniTask<bool> SearchLobby(Lobby lobbyCondition, CancellationTokenSource token, Action saveFn = null )

### Description
Search lobby to join, then get the data for p2p connect. 
saveFn is for LobbyID. By default, LobbyID is saved by Playerprefs. If you want to save LobbyID in a different location, pass the save processs to 3rd argument.
Recommend: [SearchAndCreateLobby()](../searchandcreatelobby)

```cs
using System.Threading;
using Cysharp.Threading.Tasks;
using SynicSugar.MatchMake;

public class MatchMake : MonoBehaviour {
    Lobby condition; //Create a Lobby as a condition before matchmake.
    async UniTask StartMatching(){
        CancellationTokenSource cancellToken = new CancellationTokenSource();

        bool isSuccess = await MatchMakeManager.Instance.SearchLobby(condition, cancellToken);
        
        if(!isSuccess){
            //Failuer
            return;
        }
        //Success
    }
}
```