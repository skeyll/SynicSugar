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
This is just to exit from Lobby alone during in-game(= not whole, only one battle).
For the end of battle and game, we use *[CloseSession](../ConnectHub/closesession)*.



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