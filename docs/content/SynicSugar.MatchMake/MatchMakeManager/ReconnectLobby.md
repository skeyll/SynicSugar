+++
title = "ReconnectLobby"
weight = 13
+++
## ReconnectLobby
<small>*Namespace: SynicSugar.MatchMake* <br>
*Class: MatchMakeManager* </small>

public async UniTask&lt;Result&gt; ReconnectLobby(string LobbyID,  CancellationTokenSource token = default(CancellationTokenSource))


### Description
Back to disconnected lobby. If user can't find or can't join the lobby, return false.<br> 
The User can't also go back to an empty Lobby. They can join to living one.<br>
LobbyID is saved after finishing matchmake and deleted after leaving lobby by SynicSugar.<br>
So, after starting game or moving to main manu, should get lobbyID. Then, if LobbyID exists, call this. <br>

This CancellationTokenSource is used only to cancel matchmaking.<br>
Usually we don't need pass tokensource. In this case, this function handles an exception internally and we can get just return bool result on CancelMatchMaking. If we pass it, we should TryCatch for CancelMatching.<br>
When matchmaking fails, this always returns false, not an exception.<br>


```cs
using UnityEngine;
using Cysharp.Threading.Tasks;
using SynicSugar.MatchMake;

public class MatchMake : MonoBehaviour {
    async void Start(){
        //Get Lobby ID
        string LobbyID = GetLobbyIDFromYourSavePath();
        if(string.IsNullOrEmpty(LobbyID)){
            return;
        }

        result result = await MatchMakeManager.Instance.ReconnectLobby(LobbyID);
        
        if(result == Result.Success){
            //Success
            return;
        }
        //Failuer
    }
}
```