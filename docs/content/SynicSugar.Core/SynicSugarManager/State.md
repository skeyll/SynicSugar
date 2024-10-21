+++
title = "SynicSugarState"
weight = 1
+++
## SynicSugarState
<small>*Namespace: SynicSugar*</small>
*Class: SynicSugarManager* </small>

public readonly SynicSugarState State;

### Description
User state flags.

### Property 
| API | description |
| --- | --- |
| IsLoggedIn | Whether a user are logged in via SynicSugar |
| IsMatchmaking | Whether the user is in matchmaking |
| IsInSession | Whether the user is in p2pSession |


```cs
using UnityEngine;
using SynicSugar;

public class StateCheck : MonoBehaviour {     
    private void Start()
    {
        // After call Login methods and get Success by these, this value becomes true.
        if(SynicSugarManger.Instance.State.IsLoggedIn){
            return;
        }

        // While call matchmaking methods, this value becomes true.<br />
        // After finish matchmaking and start p2p connection, this value returns false.
        if(SynicSugarManger.Instance.State.IsLoggedIn){
            return;
        }
        
        // After get Success by matchmaking APIs include for Offline, this value becomes true　until finish Session by ExitSession or CloseSession.
        if(SynicSugarManger.Instance.State.IsLoggedIn){
            return;
        }
    }
}
```