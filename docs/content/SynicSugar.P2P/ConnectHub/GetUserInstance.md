+++
title = "GetUserInstance"
weight = 9
+++
## GetUserInstance
<small>*Namespace: SynicSugar.P2P* <br>
*Class: ConnectHub* </small>

For NetworkPlayer<br>
public T GetUserInstance<T>(UserId id) <br><br>
For NetworkCommons<br>
public T GetUserInstance<T>()<br>


### Description
Get a instanse registered to ConnectHub.<br>
If we want to use this, pass true to Network class attributes.


```cs
using SynicSugar.P2P;
using UnityEngine;

[NetworkCommons(true)]
public partial class p2pSample {
    void Start(){
        ConnectHub.Instance.RegisterInstance(this);
    }
}

public class p2pSample2 {
    void p2pSampleMethod(){
        p2pSample pSample = ConnectHub.Instance.GetUserInstance<p2pSample>();
    }
}
```