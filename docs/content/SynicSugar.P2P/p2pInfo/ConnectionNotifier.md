+++
title = "ConnectionNotifier"
weight = 3
+++
## ConnectionNotifier
<small>*Namespace: SynicSugar.P2P* <br>
*Class: p2pInfo* </small>

public ConnectionNotifier ConnectionNotifier


### Description
Invoke the registered event when another user falls from a connection or joins Lobby after matchmaking.<br>

**OnTargetDisconnected** means a complete loss of a connection.<br>
In other word, the game has crashed or something problem.<br>
If a connection is temporarily lost, OnTargetEarlyDisconnected is fired (Need to check UseDisconnectedEarlyNotify in p2pConfig). At this time, some peers send a heartbeat to the Lobby to check if the user is disconnected from the Lobby and EOS will automatically try to reconnect. (If success on attempt, Restored is invoked. If not, OnClosed event called internally for p2p conenction.) <br>
When the game go to the back screen, the game keeps connection. Even if a user loses p2p for some reason, EOS will switch to the connection via Relay.<br>
If the connection cannot be re-established after such attempts, SynicSugar determines the user is **OnTargetDisconnected**. This is made in about **5 seconds** after (internal)OnClosed.<br>
**OnTargetLeaved** is invoked when target leave lobby with SynicSugar API.<br><br>
When UseDisconnectedEarlyNotify is true, SynicSugar sends HeartBeat to Lobby and disconnected peer in EarlyNotify to get notify faster. If false, these heartbeats are sent in OnClosed.<br>
SynicSugar uses RefleshPing for this heartbeat, so I recommends that we make AutoRefleshPing false when use EarlyNotify. True is fine if you do not need to know about disconnection notifications early.

### Event
| API | description |
|---|---|
| OnTargetLeaved | Invoke when another user leaves |
| OnTargetDisconnected | Invoked when another user disconnects unexpectedly |
| OnTargetConnected | Invoked when a user connects after matchmaking |
| OnTargetEarlyDisconnected | Invoke when a connection is interrupted with another peer |
| OnTargetRestored | Invoke when a connection is restored with another EarlyDisconnected peer |
| OnLobbyClosed | Invoke when the lobby is closed by host or the local user is discpnnected from the lobby  |

```cs
    public enum Reason {
        Left, Disconnected, Interrupted, LobbyClosed, Kicked, Unknown
    }
```

### Function
| API | description |
|---|---|
| Register | Set events |

```cs
using SynicSugar.P2P;
using UnityEngine;

public class p2pSample : MonoBehaviour {
    void Start(){
        p2pInfo.Instance.ConnectionNotifier.OnTargetDisconnected += OnDisconect;
        p2pInfo.Instance.ConnectionNotifier.OnTargetConnected += t => Debug.Log($"{t} Join");
    }

    void OnDisconect(UserId target){
        Debug.Log($"{target} is Disconnected / {p2pInfo.Instance.ClosedReason}");
    }
}
```