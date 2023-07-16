+++
title = "ConnectionNotifier"
weight = 5
+++
## ConnectionNotifier
<small>*Namespace: SynicSugar.P2P* <br>
*Class: p2pConfig* </small>

public ConnectionNotifier ConnectionNotifier


### Description
Invoke the registered event when another user falls from a connection or joins Lobby after matchmaking.<br>
**Disconnected** has a lag of about 5-10 seconds after a user falls in its local.

### Properity
| API | description |
|---|---|
| ClosedReason | Disconnected reason  |
| TargetUserId | UserID of Disconnected or Connected user |
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
        p2pConfig.Instance.ConnectionNotifier.Disconnected += OnDisconect;
        p2pConfig.Instance.ConnectionNotifier.Connected += () => Debug.Log($"{p2pConfig.Instance.ConnectionNotifier.TargetUserId} Join");
    }

    void OnDisconect(){
        Debug.Log($"{p2pConfig.Instance.ConnectionNotifier.TargetUserId} is Disconnected / {p2pConfig.Instance.ConnectionNotifier.ClosedReason}");
    }
}
```