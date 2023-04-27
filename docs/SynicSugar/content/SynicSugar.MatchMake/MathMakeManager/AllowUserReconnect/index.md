+++
title = "AllowUserReconnect"
weight = 2
+++
## AllowUserReconnect
public bool AllowUserReconnect

### Description
Can a disconnected user back to the lobby? This disconnect means that the app down and the user left the lobby. Just p2p disconnections are automatically reconnected. If all users fall, the lobby will be destroyed, so can't reconnect.
default is true.