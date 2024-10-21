+++
title = "ConnectionIsValid"
weight = 6
+++
## ConnectionIsValid
<small>*Namespace: SynicSugar.P2P* <br>
*Class: p2pInfo* </small>

public bool ConnectionIsValid ()


### Description
Checks if the connection has been enabled by the library or user.<br>
This does not necessarily mean that an actual connection has been established.<br>
The IsConnected flag becomes true after the user or library initiates the connection.<br>
After that, it will never become False unless we explicitly Pause connection.<br>

```cs
using SynicSugar.P2P;
using UnityEngine;
[NetworkCommons]
public partial class p2pSample : MonoBehaviour {

    public void RestartConnection(){
        if(p2pInfo.Instance.ConnectionIsValid()){
            return;
        }
        ConnectHub.Instance.RestartConnections();
    }
}
```