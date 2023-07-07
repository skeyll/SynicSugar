+++
title = "Instance"
weight = 0
+++
## Instance
<small>*Namespace: SynicSugar.P2P* <br>
*Class: ConnectHub* </small>

public static ConnectHub Instance => instance.Value;

```cs
    private static Lazy<ConnectHub> instance = new Lazy<ConnectHub>();
    public static ConnectHub Instance => instance.Value;
```

### Description
The singleton instance for ConnectHub.<br>
This script and instance is generated for each Assembly.


```cs
using SynicSugar.P2P;

public class p2pSample {
    void ConnectHubSample(){
        ConnectHub.Instance.StartPacketReceiver();
    }
}
```