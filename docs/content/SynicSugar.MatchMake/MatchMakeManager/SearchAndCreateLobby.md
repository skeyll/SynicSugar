+++
title = "SearchAndCreateLobby"
weight = 5
+++
## SearchAndCreateLobby
public async UniTask<bool> SearchAndCreateLobby(Lobby lobbyCondition, CancellationTokenSource token, Action saveFn = null )

### Description
Start MatchMake with conditions and get the data for p2p connect.
At first, search and try to join. If can't, the user create lobby as host.
If success and finish preparation p2p connect, return true. If not (by timeout or anything problem), return false.
saveFn is for LobbyID. By default, LobbyID is saved by Playerprefs. If you want to save LobbyID in a different location, pass the save processs to 3rd argument.

```cs
using System.Threading;
using Cysharp.Threading.Tasks;
using SynicSugar.MatchMake;

public class MatchMake : MonoBehaviour {
    Lobby condition; //Create a Lobby as a condition before matchmake.
    async UniTask StartMatching(){
        CancellationTokenSource cancellToken = new CancellationTokenSource();

        bool isSuccess = await MatchMakeManager.Instance.SearchAndCreateLobby(condition, cancellToken);
        
        if(!isSuccess){
            //Failuer
            return;
        }
        //Success
    }
}
```