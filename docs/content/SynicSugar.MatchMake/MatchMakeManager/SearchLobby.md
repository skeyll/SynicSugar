+++
title = "SearchLobby"
weight = 11
+++
## SearchLobby
<small>*Namespace: SynicSugar.MatchMake* <br>
*Class: MatchMakeManager* </small>

public async UniTask&lt;bool&gt; SearchLobby(Lobby lobbyCondition, CancellationTokenSource token)

### Description
Search lobby to join. If can join and exchange the data for p2p connect, return true. 

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