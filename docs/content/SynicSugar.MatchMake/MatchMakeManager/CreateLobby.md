+++
title = "CreateLobby"
weight = 12
+++
## CreateLobby
<small>*Namespace: SynicSugar.MatchMake* <br>
*Class: MatchMakeManager* </small>

public async UniTask&lt;bool&gt; CreateLobby(Lobby lobbyCondition, CancellationTokenSource token = default(CancellationTokenSource))

### Description
Create lobby as host, wait for others until timeoutSec. If the room is filled and can exchange the data for p2p, return true. <br>

This CancellationTokenSource is used only to cancel matchmaking.<br>
Usually we don't need pass tokensource. In this case, this function handles an exception internally and we can get just return bool result on CancelMatchMaking. If we pass it, we should TryCatch for CancelMatching.<br>
When matchmaking fails, this always returns false, not an exception.<br>

Recommend: [SearchAndCreateLobby()](../searchandcreatelobby)

```cs
using UnityEngine;
// using System.Threading;
using Cysharp.Threading.Tasks;
using SynicSugar.MatchMake;

public class MatchMake : MonoBehaviour {
    Lobby condition; //Create a Lobby as a condition before matchmake.
    async UniTask StartMatching(){
        bool isSuccess = await MatchMakeManager.Instance.CreateLobby(condition);

        // //try catch
        // bool isSuccess = false;
        // try{
        //     CancellationTokenSource cts = new CancellationTokenSource();
        //     //Get Success or Failuer
        //     isSuccess = await MatchMakeManager.Instance.CreateLobby(condition, cts);
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