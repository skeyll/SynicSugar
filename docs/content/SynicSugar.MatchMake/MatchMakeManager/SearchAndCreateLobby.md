+++
title = "SearchAndCreateLobby"
weight = 10
+++
## SearchAndCreateLobby
<small>*Namespace: SynicSugar.MatchMake* <br>
*Class: MatchMakeManager* </small>

##### Auto matchmaking
public async UniTask&lt;bool&gt; SearchAndCreateLobby(Lobby lobbyCondition, CancellationTokenSource token = default(CancellationTokenSource))<br>
##### Manual matchmaking
public async UniTask&lt;bool&gt; SearchAndCreateLobby(Lobby lobbyCondition, uint minLobbyMember, List&lt;AttributeData&gt; userAttributes = null, CancellationTokenSource token = default(CancellationTokenSource))


### Description
Start MatchMake with args condition and get the data for p2p connect.<br>
At first, search and try to join. If can't, the user create lobby as host.<br>
If success and finish preparation p2p connect, return true. If not (by timeout or anything problem), return false.<br>

This CancellationTokenSource is used only to cancel matchmaking. <br>
Usually we don't need pass token source. If not pass, when we call CancelMatchMaking(), we get just bool result from this method. If pass source, we need TryCatch for CancelMatching.<br>
When matchmaking fails, this always returns false, not an exception. To get result code, use *[LastResultCode](../../../SynicSugar.MatchMake/MatchMakeManager/lastresultcode)*.<br>


For Host to conclude matchmaking, pass minLobbyMember. This allows Host to kick other member, and end matchmaking after meeting members condition. In the case, the matchmaking won't have ended until Host conclude it in manual.<br>
The value is 0, less than LobbyMaxMembers or null, the matchmaking becomes Auto(Random) matchmaking. <br>
Auto does not allow Host to kick Guests, and anyone who meets the lobbyCondition can join the lobby. When the lobby is full, closes the lobby not to join and start to prepare p2p automatically.

userAttributes is for matchmaking. The user attributes of names, job and so on that is needed before P2P. <br>
These should be used just for matchmaking and the kick, the data for actual game should be exchanged via p2p for security and server bandwidth.


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