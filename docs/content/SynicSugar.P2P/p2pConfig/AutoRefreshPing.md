+++
title = "AutoRefreshPing"
weight = 8
+++
## AutoRefreshPing
<small>*Namespace: SynicSugar.P2P* <br>
*Class: p2pConfig* </small>

public bool AutoRefreshPing;


### Description
Get new ping automatically or not.<br>
Getting ping calculates a ping by calling Rpc. So, if don't need ping, should set to False. Ping can also be updated manually by p2pInfo.instance.RefreshPing().


```cs
using SynicSugar.P2P;

public class p2pConfigManager {
    void Setp2pConfig(){
        p2pConfig.Instance.AutoRefreshPing = false;
    }
}
```