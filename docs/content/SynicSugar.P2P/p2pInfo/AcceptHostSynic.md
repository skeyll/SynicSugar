+++
title = "AcceptHostSynic"
weight = 2
+++
## AcceptHostSynic
<small>*Namespace: SynicSugar.P2P* <br>
*Class: p2pInfo* </small>

public bool AcceptHostSynic;


### Description
**This is used internal.**<br>
When a user join the lobby with *[ReconnectLobby()](../../../SynicSugar.MatchMake/MatchMakeManager/reconnectlobby)*, this value become true in the local.<br>
When this value is True, Host can overwrite LocalUserId's data with remote data on Host's local only once.