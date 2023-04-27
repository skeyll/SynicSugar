+++
title = "ReconnectParticipatingLobby"
weight = 6
+++
## ReconnectParticipatingLobby
public async UniTask<bool> ReconnectParticipatingLobby(string LobbyID, CancellationTokenSource token)

### Description 
Back to disconnected lobby. If user can't find or can't join the lobby, return false.
LobbyID is saved after finishing matchmake and deleted after leaving lobby by SynicSugar function.
So, after starting game or moving to main manu, get lobbyID. Then, if the lobby ID exists, call this function. The function has *if(string.IsNullOrEmpty(LobbyID))* on the top. So, if the LobbyID is invalid, this returns false.

```cs
using System.Threading;
using Cysharp.Threading.Tasks;
using SynicSugar.MatchMake;

public class MatchMake : MonoBehaviour {
    async UniTaskVoid Start(){
        //Get Lobby ID
        string LobbyID = GetParticipatingLobbyID();
        CancellationTokenSource cancelToken = new CancellationTokenSource();

        bool isSuccess = await MatchMakeManager.Instance.ReconnectParticipatingLobby(LobbyID, cancelToken);
        
        if(isSuccess){
            //Success
            return;
        }
        //Failuer
    }
}
```