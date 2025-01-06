+++
title = "enableHostmigrationInMatchmaking"
weight = 1
+++
## enableHostmigrationInMatchmaking
<small>*Namespace: SynicSugar.MatchMake* <br>
*Class: MatchMakeManager* </small>

public bool enableHostmigrationInMatchmaking;




### Description
This determines how to handle a host's timeout from a lobby.<br><br>

There are two ways to close matchmaking: Exit and Close. If Exit is used, the host leaves the lobby, and a new host is automatically selected from the remaining users to continue matchmaking. On the other hand, if Close is used, the host destroys the lobby. Guests receive a lobby destruction notification and close matchmaking on their end as LobbyClosed.<br>

When the host's matchmaking times out, the system internally calls either Exit or Close, but you cannot specify which method to use. Therefore, SynicSugar uses this value to decide the timeout process.<br>

For example, suppose only the host defines the lobby's conditions and starts matchmaking by calling CreateLobby, while guests join using an ID or similar means. In this case, the conditions should be determined solely by the host. Therefore, the lobby should not persist after a timeout. In this scenario, you should set enableHostMigrationInMatchmaking = true before calling CreateLobby (or at the latest, before a timeout).<br>

If the game's rules are system-defined or if it does not matter who becomes the host, it is fine to set it to false. It is also recommended to set it to false when joining as a guest. Even if you become a new host via host migration, the value of enableHostMigrationInMatchmaking specified in the Manager at the time of the timeout takes precedence.


```cs
using UnityEngine;
using SynicSugar.MatchMake;

public class MatchmakingSample : MonoBehaviour {
    private void Start(){
        MatchMakeManager.Instance.enableHostmigrationInMatchmaking = true;
    }
}
```