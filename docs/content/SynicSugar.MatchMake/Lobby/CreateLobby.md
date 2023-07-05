+++
title = "CreateLobby"
weight = 12
+++
## CreateLobby
<small>*Namespace: SynicSugar.MatchMake* <br>
*Class: MatchMakeManager* </small>

public async UniTask&lt;bool&gt; CreateLobby(Lobby lobbyCondition, CancellationTokenSource token, Action saveFn = null )

### Description
Create lobby as host, wait for others until timeoutSec. If the room is filled and can exchange the data for p2p, return true. 
Recommend: [SearchAndCreateLobby()](../searchandcreatelobby)

```cs
using System.Threading;
using Cysharp.Threading.Tasks;
using SynicSugar.MatchMake;

public class MatchMake : MonoBehaviour {
    Lobby condition; //Create a Lobby as a condition before matchmake.
    async UniTask StartMatching(){
        CancellationTokenSource cancellToken = new CancellationTokenSource();

        bool isSuccess = await MatchMakeManager.Instance.CreateLobby(condition, cancellToken);
        
        if(!isSuccess){
            //Failuer
            return;
        }
        //Success
    }
}
```