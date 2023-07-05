+++
title = "ReconnecLobby"
weight = 13
+++
## ReconnecLobby
<small>*Namespace: SynicSugar.MatchMake* <br>
*Class: MatchMakeManager* </small>

public async UniTask&lt;bool&gt; ReconnecLobby(string LobbyID, CancellationTokenSource token)


### Description
Back to disconnected lobby. If user can't find or can't join the lobby, return false.
LobbyID is saved after finishing matchmake and deleted after leaving lobby by SynicSugar.
So, after starting game or moving to main manu, should get lobbyID. Then, if LobbyID exists, call this. 


```cs
using System.Threading;
using Cysharp.Threading.Tasks;
using SynicSugar.MatchMake;

public class MatchMake : MonoBehaviour {
    async void Start(){
        //Get Lobby ID
        string LobbyID = GetLobbyIDFromYourSavePath();
        if(string.IsNullOrEmpty(LobbyID)){
            return;
        }
        CancellationTokenSource cancelToken = new CancellationTokenSource();

        bool isSuccess = await MatchMakeManager.Instance.ReconnecLobby(LobbyID, cancelToken);
        
        if(isSuccess){
            //Success
            return;
        }
        //Failuer
    }
}
```