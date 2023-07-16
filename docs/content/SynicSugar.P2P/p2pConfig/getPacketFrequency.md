+++
title = "getPacketFrequency"
weight = 3
+++
## getPacketFrequency
<small>*Namespace: SynicSugar.P2P* <br>
*Class: p2pConfig* </small>

public GetPacketFrequency getPacketFrequency 


```cs
    public enum GetPacketFrequency {
        PerSecondFPS, PerSecond100, PerSecond50, PerSecond25
    }
```

### Description
Interval [ms] to get packet from the receiving buffer.<br>
Whichever we choose, this can't exceed the FPS of the game. Excessive receiving will result in a drop in FPS. **I recommend PerSecond50 for Mobile games.**


Can set this value on UnityEditor.


```cs
using SynicSugar.P2P;

public class p2pConfigManager {
    void Setp2pConfig(){
        p2pConfig.Instance.getPacketFrequency = p2pConfig.GetPacketFrequency.PerSecond50;
    }
}
```