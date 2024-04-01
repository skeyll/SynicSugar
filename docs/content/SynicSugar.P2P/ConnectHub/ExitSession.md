+++
title = "ExitSession"
weight = 6
+++
## ExitSession
<small>*Namespace: SynicSugar.P2P* <br>
*Class: ConnectHub* </small>

public async UniTask&lt;bool&gt; ExitSession(CancellationTokenSource cancelToken = default(CancellationTokenSource))


### Description
Stop the packet receiver, close all connections, remove the notify events and destroy ConnectManager object. Then, the user leaves Lobby.<br>
The last user closes the lobby in Backend.


```cs
using SynicSugar.P2P;
using System.Threading;

public class p2pSample {
    public async void ConnectHubSample(){
        CancellationTokenSource token = new CancellationTokenSource();
        await ConnectHub.Instance.ExitSession(token);
    }
}
```