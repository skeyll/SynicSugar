+++
title = "GetUserInstance"
weight = 9
+++
## GetUserInstance
<small>*Namespace: SynicSugar.P2P* <br>
*Class: ConnectHub* </small>

public T GetUserInstance&lt;T&gt;(UserId id) 


### Description
Get a target instance registered to ConnectHub.<br>
If we want to use this, pass true to NetworkPlayer class attributes.


```cs
using SynicSugar.P2P;

[NetworkPlayer(true)]
public partial class p2pSample {
}

public class p2pSample2 {
    void p2pSampleMethod(){
        p2pSample pSample = ConnectHub.Instance.GetUserInstance<p2pSample>(targetId);
    }
}
```