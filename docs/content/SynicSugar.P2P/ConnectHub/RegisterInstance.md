+++
title = "RegisterInstance"
weight = 8
+++
## RegisterInstance
<small>*Namespace: SynicSugar.P2P* <br>
*Class: ConnectHub* </small>

For NetworkPlayer<br>
public void RegisterInstance(UserId id, T classInstance)<br><br>
For NetworkCommons<br>
public void RegisterInstance(T classInstance)<br>


### Description
Register Instance to ConnectHub to get a packet.<br>
On NetworkPlayer, this is called on setting UserID, but on NetworkCommons, we need call this by hand on Start or after.


```cs
using SynicSugar.P2P;
using UnityEngine;

[NetworkCommons]
public partial class p2pSample {
    void Start(){
        ConnectHub.Instance.RegisterInstance(this);
    }
}
```