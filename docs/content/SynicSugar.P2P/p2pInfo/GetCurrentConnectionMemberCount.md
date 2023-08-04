+++
title = "GetAllConnectionMemberCount"
weight = 8
+++
## GetAllConnectionMemberCount
<small>*Namespace: SynicSugar.P2P* <br>
*Class: p2pInfo* </small>

public int GetAllConnectionMemberCount()<br>

### Description
Get all member count that is current and past participation member count instead of just current.


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