+++
title = "GetActiveReceiverType"
weight = 6
+++
## GetActiveReceiverType
<small>*Namespace: SynicSugar.P2P* <br>
*Class: p2pInfo* </small>

public ReceiverType GetActiveReceiverType ()<br>

```cs
    public enum ReceiverType {
        None, FixedUpdate, Update, LateUpdate, Synic
    }
```

### Description
Gets the currently valid (active) packet receiver type.<br>


```cs
using SynicSugar.P2P;
using UnityEngine;
[NetworkCommons]
public partial class p2pSample : MonoBehaviour {

    public void Start(){
        if(ReceiverType.None == p2pInfo.Instance.GetActiveReceiverType()){
            ConnectHub.Instance.StartPacketReceiver(PacketReceiveTiming.FixedUpdate, 5);
            //p2pInfo.Instance.GetActiveReceiverType() becomes ReceiverType.FixedUpdate
        }
    }
}
```