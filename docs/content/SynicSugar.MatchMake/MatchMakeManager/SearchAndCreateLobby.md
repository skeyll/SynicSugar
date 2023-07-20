+++
title = "SearchAndCreateLobby"
weight = 10
+++
## SearchAndCreateLobby
<small>*Namespace: SynicSugar.MatchMake* <br>
*Class: MatchMakeManager* </small>

public async UniTask&lt;bool&gt; SearchAndCreateLobby(Lobby lobbyCondition, CancellationTokenSource token = default(CancellationTokenSource))


### Description
Start MatchMake with args condition and get the data for p2p connect.<br>
At first, search and try to join. If can't, the user create lobby as host.<br>
If success and finish preparation p2p connect, return true. If not (by timeout or anything problem), return false.<br>

This CancellationTokenSource is used only to cancel matchmaking. <br>
Usually we don't need pass token source. If not pass, when we call CancelMatchMaking(), we get just bool result from this method. If pass source, we need TryCatch for CancelMatching.<br>
When matchmaking fails, this always returns false, not an exception.<br>

Recommend this for Matchmaking.

```cs
using UnityEngine;
// using System.Threading;
using Cysharp.Threading.Tasks;
using SynicSugar.MatchMake;

public class MatchMake : MonoBehaviour {
    Lobby condition; //Create a Lobby as a condition before matchmake.
    async UniTask StartMatching(){
        bool isSuccess = await MatchMakeManager.Instance.SearchAndCreateLobby(condition);

        // //try catch
        // bool isSuccess = false;
        // try{
        //     CancellationTokenSource cts = new CancellationTokenSource();
        //     //Get Success or Failuer
        //     isSuccess = await MatchMakeManager.Instance.SearchAndCreateLobby(condition, cts);
        // }catch(OperationCanceledException){
        //     //Cancel matchmaking
        //     isSuccess = false;
        // }
        
        if(!isSuccess){
            //Failuer
            return;
        }
        //Success
    }
}
```