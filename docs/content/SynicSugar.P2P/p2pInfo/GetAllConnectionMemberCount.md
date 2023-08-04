+++
title = "GetCurrentConnectionMemberCount"
weight = 7
+++
## GetCurrentConnectionMemberCount
<small>*Namespace: SynicSugar.P2P* <br>
*Class: p2pInfo* </small>

public int GetCurrentConnectionMemberCount()<br>

### Description
Get member count in just current connection.


```cs
using SynicSugar.P2P;
using UnityEngine;
using UnityEngine.UI;

public class p2pSample : MonoBehaviour {
    [SerializeField] Text memberCount;

    public void UpdateMemberCount(){
        memberCount.text = $"Limit: {MatchMakeManager.Instance.GetMaxLobbyMemberCount()} / Room {MatchMakeManager.Instance.GetMaxLobbyMemberCount() - p2pInfo.Instance.GetAllConnectionMemberCount()} / Current {p2pInfo.Instance.GetCurrentConnectionMemberCount()}";
    }
}
```