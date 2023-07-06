+++
title = "intervalsendtoAll"
weight = 1
+++
## interval_sendToAll
<small>*Namespace: SynicSugar.P2P* <br>
*Class: p2pConfig* </small>

public int interval_sendToAll

This is used like **p2pConfig.Instance.XXX()**.


### Description
Interval [ms] of Rpc to send to each player.<br>
This interval is made so that the sending buffer is not full.<br>
*This API is most likely a change.*<br>
**Recommend: 3ms-**

Can set this value on UnityEditor.


```cs
using SynicSugar.P2P;

public class p2pConfigManager {
    void Setp2pConfig(){
        p2pConfig.Instance.interval_sendToAll = 10;
    }
}
```