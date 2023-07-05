+++
title = "SearchAndCreateLobby"
weight = 10
+++
## SearchAndCreateLobby
<small>*Namespace: SynicSugar.MatchMake* <br>
*Class: MatchMakeManager* </small>

public async UniTask&lt;bool&gt; SearchAndCreateLobby(Lobby lobbyCondition, CancellationTokenSource token)


### Description
Start MatchMake with args condition and get the data for p2p connect.
At first, search and try to join. If can't, the user create lobby as host.
If success and finish preparation p2p connect, return true. If not (by timeout or anything problem), return false.

Recommend this for Matchmaking.

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