+++
title = "p2pConnectorForOtherAssembly"
weight = 6
+++

## ConnectHub
<small>*Namespace: SynicSugar.P2P*</small>


### Description
This script is Mono's Singleton attached to ConnenctManager.<br>
*Used internally.*<br>

Just for hub between Assembly.<br>
We can't call the main-Assembly from own-assemblies.
And we didn't bother to generate this, so use this. And we didn't bother to generate this, so we use this for *[ConnectHub](../connecthub/)*.



### Properity
| API | description |
|---|---|
| ScoketName | Unique strings + null generated on matchmaking |
| p2pToken | Cancel token for PacketReceiver |
| receiverInterval | Interval for PacketReceiver |


### Function 
| API | description |
|---|---|
| PauseConnections |  |
| RestartConnections |  |
| ExitSession |  |
| CloseSession |  |
| GetPacketFromBuffer |  |