+++
title = "StartSynicReceiver"
weight = 2
+++
## StartSynicReceiver
<small>*Namespace: SynicSugar.P2P* <br>
*Class: ConnectHub* </small>

public void StartSynicReceiver()


### Description
Start Packet Receiver (in burst FPS) to get only Synic packets from the receiving buffer.<br>
This cannot be called with other Receiver at the same time. If start the other Receiver, ConenctHub stop this Receiver automatically before start the new one.<br>
Need the Instance of each Class to get the packet.<br>
So, genarete the components by *[SynicObject](../synicobject/)*, or set UserID in or after Unity.Start to Register the instanced to ConnectHub.<br>
NetworkComponents need call *[RegisterInstance](../ConnectHub/registerinstance)* in or after Unity.Start by hand.


```cs
using SynicSugar.P2P;
using Cysharp.Threading.Tasks;

public class p2pSample {
    async void UniTaskVoid Start(){
        //This user is Reconnecter
        if(p2pInfo.Instance.IsReconnecter){
                //To get SynicPacket.
                ConnectHub.Instance.StartSynicReceiver();
                //This flag(HasReceivedAllSyncSynic) cannot be used at the same time. Once it returns True, it returns False again.
                await UniTask.WaitUntil(() => p2pInfo.Instance.HasReceivedAllSyncSynic);
            }
        }
        ConnectHub.Instance.StartSynicReceiver();
    }
}
```