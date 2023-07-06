+++
title = "autoSyncInterval"
weight = 2
+++
## autoSyncInterval
<small>*Namespace: SynicSugar.P2P* <br>
*Class: p2pConfig* </small>

public int autoSyncInterval

This is used like **p2pConfig.Instance.XXX()**.


### Description
Interval [ms] of SyncVar to send new updated value to each player.<br>
It's sent by *SendPacketToAll*. So this is also affected by *[interval_sendToAll](../p2pConfig/intervalsendtoall)*.<br>

Sending the position every changes will quickly congest the bandwidth. Therefore, SynicSugar has interval to synchronize SyncVar.<br>

About Synchronization<br>
1...Neutral -> not sync<br>
2...Change value (Including initialization) -> **Sync**<br>
3...During autoSyncInterval, change value -> not Sync <br>
4...The value has been changed on the moment autoSyncInterval passes -> **Sync, then wait autoSyncInterval again and loop 3 and 4 step**<br>
5...The value has't been changed -> back to 1

**Recommend: 1000-3000ms**

Can set this value on UnityEditor.


```cs
using SynicSugar.P2P;

public class p2pConfigManager {
    void Setp2pConfig(){
        p2pConfig.Instance.autoSyncInterval = 1500;
    }
}
```