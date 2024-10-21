+++
title = "ExitSession"
weight = 6
+++
## ExitSession
<small>*Namespace: SynicSugar.P2P* <br>
*Class: ConnectHub* </small>

public async UniTask&lt;bool&gt; ExitSession(bool destroyManager = true, bool cleanupMemberCountChanged = false, CancellationToken cancelToken = default(CancellationToken))


### Description
Stop the packet receiver, close all connections, remove the notify events and destroy ConnectManager object. Then, the user leaves Lobby.<br>
The last user closes the lobby in Backend.<br><br>

destroyManager: If true, destroy NetworkManager after exit lobby.<br>
cleanupMemberCountChanged: Need to call MatchMakeManager.Instance.MatchMakingGUIEvents.LobbyMemberCountChanged(id, false) after exit lobby?<br>
cancelToken: ancel token for this task


```cs
using SynicSugar.P2P;
using System.Threading;

public class p2pSample {
    public async void ConnectHubSample(){
        await ConnectHub.Instance.ExitSession();
    }
}
```