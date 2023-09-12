+++
title = "SearchLobby"
weight = 11
+++
## SearchLobby
<small>*Namespace: SynicSugar.MatchMake* <br>
*Class: MatchMakeManager* </small>

##### Auto matchmaking
public async UniTask&lt;bool&gt; SearchLobby(Lobby lobbyCondition, CancellationTokenSource token = default(CancellationTokenSource))
##### Manual matchmaking
public async UniTask&lt;bool&gt; SearchLobby(Lobby lobbyCondition, uint minLobbyMember, List&lt;AttributeData&gt; userAttributes = null, CancellationTokenSource token = default(CancellationTokenSource))

### Description
Search lobby to join. If can join and exchange the data for p2p connect, return true. 

This CancellationTokenSource is used only to cancel matchmaking.<br>
Usually we don't need pass tokensource. In this case, this function handles an exception internally and we can get just return bool result on CancelMatchMaking. If we pass it, we should TryCatch for CancelMatching.<br>
When matchmaking fails, this always returns false, not an exception. To get result code, use *[LastResultCode](../MatchMakeManager/lastresultcode)*.<br>


After host-migration for Host to conclude matchmaking, pass minLobbyMember.<br>
The value is 0, less than LobbyMaxMembers or null, the matchmaking becomes Auto(Random) matchmaking. <br>
Auto does not allow Host to kick Guests, and anyone who meets the lobbyCondition can join the lobby. When the lobby is full, closes the lobby not to join and start to prepare p2p automatically.

userAttributes is for matchmaking. The user attributes of names, job and so on that is needed before P2P. <br>
These should be used just for matchmaking and the kick, the data for actual game should be exchanged via p2p for security and server bandwidth.

Recommend: *[SearchAndCreateLobby()](../searchandcreatelobby)*

```cs
using UnityEngine;
// using System.Threading;
using Cysharp.Threading.Tasks;
using SynicSugar.MatchMake;

public class MatchMake : MonoBehaviour {
    Lobby condition; //Create a Lobby as a condition before matchmake.
    async UniTask StartMatching(){
        
        bool isSuccess = await MatchMakeManager.Instance.SearchLobby(condition);

        // //try catch
        // bool isSuccess = false;
        // try{
        //     CancellationTokenSource cts = new CancellationTokenSource();
        //     //Get Success or Failuer
        //     isSuccess = await MatchMakeManager.Instance.SearchLobby(condition, cts);
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