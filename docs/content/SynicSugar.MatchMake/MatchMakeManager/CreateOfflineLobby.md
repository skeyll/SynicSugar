+++
title = "CreateOfflineLobby"
weight = 20
+++
## CreateOfflineLobby
<small>*Namespace: SynicSugar.MatchMake* <br>
*Class: MatchMakeManager* </small>

public async UniTask&lt;bool&gt; CreateOfflineLobby(Lobby lobbyCondition, OfflineMatchmakingDelay delay, ListList&lt;AttributeData&gt; userAttributes = null, CancellationTokenSource token = default(CancellationTokenSource))

### Description
Create lobby just in Local to use scripts in tutorial and offline mode. This lobby is not connected to the network. <br>

This Lobby features
- Offline
- Follow the same step as actual matchmaking to create offline-lobby and complete preparations for p2p
- Only single user in the lobby
- No VC

This API instantiate RemoteUserIds, but that is empty. So, cannot specified by Index. <br>
If we use RemoteUserId with FOREACE, or FOR and RemoteUserId.Count, can completely share the online projects as offline projects.<br><br>

When destory, call MatchMakeManager.Instance.DestoryOfflineLobby, or Destroy(MatchMakeManager.Instance) and your Delete method for Reconnection LobbyID.

```cs
using UnityEngine;
using Cysharp.Threading.Tasks;
using SynicSugar.MatchMake;

public class MatchMake : MonoBehaviour {
    Lobby condition; //Create a Lobby as a condition before matchmake.
    async UniTask StartMatching(){
        OfflineMatchmakingDelay delay = OfflineMatchmakingDelay.NoDelay;
        // To simulate actual matchmaking, we can set delay ms.
        // Pass new OfflineMatchmakingDelay(0, 0, 0, 0) or OfflineMatchmakingDelay.NoDelay,
        // this immediately create lobby and returns true without async process and calling MatchMakingGUIEvents. 
        // OfflineMatchmakingDelay delay = new OfflineMatchmakingDelay(2000, 1000, 1000, 1000);

        //This always return true.
        bool isSuccess = await MatchMakeManager.Instance.CreateOfflineLobby(matchConditions.GetLobbyCondition(2), delay);

        if(!isSuccess){
            //Failuer
            return;
        }
        //Success
    }
}
```