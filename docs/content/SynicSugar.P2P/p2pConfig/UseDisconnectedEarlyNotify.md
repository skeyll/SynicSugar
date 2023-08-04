+++
title = "UseDisconnectedEarlyNotify"
weight = 5
+++
## UseDisconnectedEarlyNotify
<small>*Namespace: SynicSugar.P2P* <br>
*Class: p2pConfig* </small>

public bool UseDisconnectedEarlyNotify 


### Description
Notify loss connection on temporarily, instead of losing it on completely. After this notification, SynicSugar try to reconnection automatically, and if the connect still cannot be restored, fires Disconnected notify.


```cs
using SynicSugar.P2P;

public class p2pConfigManager {
    void Setp2pConfig(){
        p2pConfig.Instance.UseDisconnectedEarlyNotify = true;
    }
}
```