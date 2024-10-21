+++
title = "CloseSession"
weight = 7
+++
## CloseSession
<small>*Namespace: SynicSugar.P2P* <br>
*Class: ConnectHub* </small>

public async UniTask&lt;bool&gt; CloseSession(bool destroyManager = true, bool cleanupMemberCountChanged = false, CancellationToken cancelToken = default(CancellationToken))


### Description
Stop the packet receiver, close all connections, remove the notify events and destroy ConnectManager object. Then, Host closees and Guest leaves the lobby. <br>
When Host closes Lobby, Guests are automatically kicked out from the Lobby.<br>

destroyManager: If true, destroy NetworkManager after exit lobby.<br>
cleanupMemberCountChanged: Need to call MatchMakeManager.Instance.MatchMakingGUIEvents.LobbyMemberCountChanged(id, false) after exit lobby?<br>
cancelToken: ancel token for this task


```cs
using SynicSugar.P2P;
using System.Threading;
using Cysharp.Threading.Tasks;

public class p2pSample {
    public async UniTask ConnectHubSample(){
        await ConnectHub.Instance.CloseSession();
    }
}
```