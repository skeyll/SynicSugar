+++
title = "CloseSession"
weight = 7
+++
## CloseSession
<small>*Namespace: SynicSugar.P2P* <br>
*Class: ConnectHub* </small>

public async UniTask&lt;bool&gt; CloseSession(CancellationTokenSource cancelToken = default(CancellationTokenSource))


### Description
Stop the packet receiver, close all connections, remove the notify events and destroy ConnectManager object. Then, Host closees and Guest leaves the lobby.


```cs
using SynicSugar.P2P;
using System.Threading;
using Cysharp.Threading.Tasks;

public class p2pSample {
    public async UniTask ConnectHubSample(){
        CancellationTokenSource token = new CancellationTokenSource();
        await ConnectHub.Instance.CloseSession(token);
    }
}
```