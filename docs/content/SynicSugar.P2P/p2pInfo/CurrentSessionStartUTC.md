+++
title = "CurrentSessionStartUTC"
weight = 2
+++
## CurrentSessionStartUTC
<small>*Namespace: SynicSugar.P2P* <br>
*Class: p2pInfo* </small>

public DateTime CurrentSessionStartUTC { get; internal set; }


### Description
Date time when this LOCAL user starts current session.<br><br>

This time is the UTC just before the matchmaking results are returned. When matchmaking is decided and all preparations are complete, the LobbyID and UTC are saved in Application.persistentDataPath/ss_sessiondata.dat. The in-game timestamp is calculated based on the difference between this reference time and the current UTC.<br>
In P2P, there is no server, so server time cannot be used for the game. Additionally, Ping and RTC can be unreliable due to processing congestion, so SynicSugar saves the time when matchmaking finished on each device and uses it as a reference time.<br>
When reconnecting, if this value exists locally, it is used again. If it doesn't exist, an estimated start time is calculated from the elapsed time timestamp sent by the host and set to this value.<br><br>


```cs
using SynicSugar.P2P;
using UnityEngine;
[NetworkPlayer]
public partial class p2pSample : MonoBehaviour {
    void Start(){
        //For standard
        Move(p2pInfo.Instance.GetSessionTimestamp());
        
        //To need rigorous time
        Fire(p2pInfo.Instance.GetSessionTimestampInMs());
    }
    [Rpc]
    void Move(uint timestamp){
        Debug.Log($"Lag is {p2pInfo.Instance.GetSessionTimestamp() - timestamp} sec");
    }
    [Rpc]
    void Fire(double timestamp){
        Debug.Log($"Lag is {p2pInfo.Instance.GetSessionTimestampInMs() - timestamp} ms");
    }
}
```