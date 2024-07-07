+++
title = "SetRelayControl"
weight = 7
+++
## SetRelayControl
<small>*Namespace: SynicSugar.P2P* <br>
*Class: p2pConfig* </small>

public void SetRelayControl(RelayControl relay)


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
Set how relay servers are to be used. This setting does not immediately apply to existing connections, but may apply to existing connections if the connection requires renegotiation.<br> 
AllowRelay is default. In default, if the connection can be made via p2p, users connect directly; if it fails NAT Punch through, users use Relay(AWS) for the connection.<br>
If it is set to anything other than AllowRelays, SetRelayControl is automatically called before the first connection. If SetRelayControl() is called after the connection, connection will switch between via Relay and p2p when the connect is not stable, so it is better to change this value in the editor or just before or after matching starts.<br> 
SetRelayControll could be called before matching, but since SynicSugar duplicates the switching process before connecting, it may be better to change this value directly.


```cs
using SynicSugar.P2P;

public class p2pConfigManager {
    void Setp2pConfig(){
        p2pConfig.Instance.SetRelayControl(RelayControl.ForceRelays);
    }
}
```