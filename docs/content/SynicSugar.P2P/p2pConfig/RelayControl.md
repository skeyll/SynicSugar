+++
title = "RelayControl"
weight = 7
+++
## RelayControl
<small>*Namespace: SynicSugar.P2P* <br>
*Class: p2pConfig* </small>

public RelayControl relayControl = RelayControl.AllowRelays;

```cs
    public enum RelayControl {
        /// <summary>
        /// Peer connections will never attempt to use relay servers. Clients with restrictive NATs may not be able to connect to peers.
        /// </summary>
        NoRelays, 
        /// <summary>
        /// Peer connections will attempt to use relay servers, but only after direct connection attempts fail. This is the default value if not changed.
        /// </summary>
        AllowRelays, 
        /// <summary>
        /// Peer connections will only ever use relay servers. This will add latency to all connections, but will hide IP Addresses from peers.
        /// </summary>
        ForceRelays
    }
```

### Description
Set the way to connection.<br>
AllowRelay is default. In default, if the connection can be made directly, users connect by p2p; if it fails NAT Punch through, users use Relay(AWS) for the connection.<br>
Users with settings NoRelay and ForceRelays cannot connect.<br>
If we switch this value, set this before matchmaking or on Editor. If so, the library automatically switches relay setting just before conenction.<br>
SetRelayControll could be called before matching, but since SynicSugar duplicates the switching process before connecting, it may be better to change this value directly.


```cs
using SynicSugar.P2P;

public class p2pConfigManager {
    void Setp2pConfig(){
        p2pConfig.Instance.relayControl = RelayControl.ForceRelays;
    }
}
```