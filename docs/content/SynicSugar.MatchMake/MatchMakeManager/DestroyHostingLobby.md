+++
title = "DestroyHostingLobby"
weight = 9
+++
## DestroyHostingLobby
public async UniTask<bool> DestroyHostingLobby(CancellationTokenSource token, Action deleteFn = null){

### Description
Destroy Hosting Lobby. When use this function, HostMigration isn't performed in the lobby. If success, return true, and delete LobbyID from a save place.

```cs
using System.Threading;
using Cysharp.Threading.Tasks;
using SynicSugar.MatchMake;

public class MatchMake : MonoBehaviour {
    async UniTask LeaveLobby(){
        CancellationTokenSource cancellToken = new CancellationTokenSource();
        bool isSuccess = await MatchMakeManager.Instance.DestroyHostingLobby(cancellToken);
        
        if(isSuccess){
            //Success
            return;
        }
    }
}
```