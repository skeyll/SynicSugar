+++
title = "PingAutoRefreshRateSec"
weight = 9
+++
## PingAutoRefreshRateSec
<small>*Namespace: SynicSugar.P2P* <br>
*Class: p2pConfig* </small>

public int PingAutoRefreshRateSec


### Description
Interval sec to update ping automatically.<br>


```cs
using SynicSugar.P2P;

public class p2pConfigManager {
    void Setp2pConfig(){
        p2pConfig.Instance.PingAutoRefreshRateSec = 100;
    }
}
```