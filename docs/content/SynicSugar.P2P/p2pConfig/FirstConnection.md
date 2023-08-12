+++
title = "FirstConnection"
weight = 6
+++
## FirstConnection
<small>*Namespace: SynicSugar.P2P* <br>
*Class: p2pConfig* </small>

public FirstConnectionType FirstConnection;


### Description
Delay time to return true after matchmaking.<br>
After the connection is established, EOS has a lag before actual communication is possible. This is the setting of how to handle it.<br>

```cs
public enum FirstConnectionType{
    /// <summary>
    /// Return true after getting Ping. The first connection is sent on SynicSugar, so this is reliable but has a lag.
    /// </summary>
    Strict, 
    /// <summary>
    /// Return true after just sending connect request. Other peers will discard the initial some packets that the user sends during about 1sec after getting true. (Depends on the ping)
    /// </summary>
    Casual, 
    /// <summary>
    /// Return true after just sending connect request. Packets in 10 sec after matching are stored in the receive buffer even if the peer haven't accept the connection.
    /// </summary>
    TempDelayedDelivery, 
    /// <summary>
    /// Return true after just sending connect request. All packets are stored in the receive buffer even if the peer haven't accept the connection. PauseConnections() stops the work.
    /// </summary>
    DelayedDelivery
}

```

```cs
using SynicSugar.P2P;

public class p2pConfigManager {
    void Setp2pConfig(){
        p2pConfig.Instance.FirstConnection = p2pConfig.FirstConnectionType.Strict;
    }
}
```