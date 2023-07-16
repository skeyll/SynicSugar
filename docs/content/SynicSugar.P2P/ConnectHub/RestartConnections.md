+++
title = "RestartConnections"
weight = 5
+++
## RestartConnections
<small>*Namespace: SynicSugar.P2P* <br>
*Class: ConnectHub* </small>

public void RestartConnections()


### Description
Restart to receive packets to the receiving buffer and get a packet from there.<br>
This is for *[PauseConnections](../ConnectHub/pauseconnections)*


```cs
using SynicSugar.P2P;
using Cysharp.Threading.Tasks;

public class p2pSample {
    void ConnectHubSample(){
        ConnectHub.Instance.RestartConnections();
    }
}
```