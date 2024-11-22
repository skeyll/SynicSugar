+++
title = "SessionType"
weight = 2
+++
## SessionType
<small>*Namespace: SynicSugar.P2P* <br>
*Class: p2pInfo* </small>

public SessionType SessionType { get; internal set; }

```cs
namespace SynicSugar.P2P {
    public enum SessionType {
        /// <summary>
        /// Not in session.
        /// </summary>
        None,
        /// <summary>
        /// In session online with other users
        /// </summary>
        OnlineSession,
        /// <summary>
        /// In pseudo-session mode without network connection (single-player simulation).
        /// </summary>
        OfflineSession,
        /// <summary>        
        /// The lobby was closed by host or the local user disconnected from the lobby, causing the (p2p) session to end.
        /// </summary>
        InvalidSession
    }
}
```

### Description
The type of current session.<br><br>
p2pInfo.Instance.SessionType == SessionType.OfflineSession is equivalent to p2pInfo.Instance.AllUserIds.Count == 1 and MatchMakeManager.Instance.GetCurrentLobbyID() == "OFFLINEMODE".



```cs
using Cysharp.Threading.Tasks;
using SynicSugar.P2P;
using UnityEngine;
[NetworkPlayer]
public partial class p2pSample : MonoBehaviour {
    public void CloseLobby(){
        if(p2pInfo.Instance.SessionType == SessionType.OfflineSession)
        { 
            ConnectHub.Instance.DestoryOfflineLobby().Forget();
        }else
        {
            ConnectHub.Instance.CloseSession().Forget();
        }
    }
}
```