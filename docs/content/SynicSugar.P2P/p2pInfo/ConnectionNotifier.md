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

**Disconnected** means a complete loss of a connection.<br>
In other word, the game has crashed or something problem.<br>
If a connection is temporarily lost, EOS will automatically try to reconnect. When the game go to the back screen, the connections keep the same state. Even if a user loses p2p for some reason, EOS will switch to the connection via Relay.<br>
If the connection cannot be re-established after such attempts, SynicSugar determine the user is **Disconnected**. This is made in about **5 seconds**.<br>

### Properity
| API | description |
|---|---|
| Disconnected | Invoked when another user disconnects unexpectedly |
| Connected | Invoked when a user connects after matchmaking |

```cs
    public enum Reason {
        Left, Disconnected, Unknown
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
        p2pInfo.Instance.ConnectionNotifier.Disconnected += OnDisconect;
        p2pInfo.Instance.ConnectionNotifier.Connected += () => Debug.Log($"{p2pConfig.Instance.LastConnectedUsersId} Join");
    }

    void OnDisconect(){
        Debug.Log($"{p2pInfo.Instance.LastDisconnectedUsersId} is Disconnected / {p2pInfo.Instance.ClosedReason}");
    }
}
```