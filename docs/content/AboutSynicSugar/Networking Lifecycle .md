+++
title = "Nerworking Lifecycle"
weight = 3
+++
# Networking Lifecycle
In the current version of SynicSugar, the state of matchmaking and P2P sessions are clearly distinguished.

### IsMatchMaking and IsLooking
To start networking process, call the matching API. This process is managed by two flags internally: IsMatchMaking and IsLooking.

#### 1. IsMatchMaking = true
When the matchmaking API is called, the internal IsMatchMaking flag is set to true. This is the same even in offline mode. In this state, it is not possible to create a new match, but several APIs to end the match will be available.

#### 2. IsLooking = true
Right after matchmaking starts, the IsLooking flag is set to true. This indicates that the system is looking for an opponent. This flag is set to true for both host and guest, and also in CreateOfflineLobby, which simulates online matchmaking. In online matchmaking, a timer for the timeout starts immediately after this.

#### 3. IsLooking = false
If an opponent is found or the host closes the lobby, the flag will be set to false, and preparations for transitioning to P2P connection will begin.

### IsMatchMaking and IsInSession
When the matchmaking is finished, the player moves to the “Session” state, which is the only state in which communication is possible in SynicSugar. This is true even in offline mode.

#### 1. IsMatchMaking = false, IsInSession = true
When the P2P connection is successfully established and necessary information is exchanged, the matchmaking phase ends and transitions to IsInSession. IsInSession will only be true if matchmaking is successful. Some APIs in the SynicSugar.P2P namespace will return Result.InvalidAPICall if called when IsInSession is not true.<br>

The state "Session" indicates that the system is ready for communication at any time, but it does not represent the actual communication state. <br>

To check if communication is occurring or if it is paused, p2pInfo.Instance.IsConnected should be used. Right after the session transition, the connection will be automatically established, so p2pInfo.Instance.IsConnected will be true. To stop the connection, you can use Pause via ConnectHub.<br>

In offline mode, the state is in the "Session" state, but since no actual connection is made, IsConnected will always be false, and some connection-related APIs will be restricted.


Online
```cs
SynicSugarManger.Instance.State.IsMatchmaking = false;
SynicSugarManger.Instance.State.IsInSession = true;
p2pInfo.Instance.SessionType = SessionType.OnlineSession;
p2pInfo.Instance.IsConnected = true;
```

Offline
```cs
SynicSugarManger.Instance.State.IsMatchmaking = false;
SynicSugarManger.Instance.State.IsInSession = true;
p2pInfo.Instance.SessionType = SessionType.OfflineSession;
p2pInfo.Instance.IsConnected = false;
```
        
Failure in matchmaking
```cs
SynicSugarManger.Instance.State.IsMatchmaking = false;
SynicSugarManger.Instance.State.IsInSession = false;
p2pInfo.Instance.SessionType = SessionType.None;
p2pInfo.Instance.IsConnected = false;
```