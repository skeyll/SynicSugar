+++
title = "isLocalUserId"
weight = 27
+++
## isLocalUserId
<small>*Namespace: SynicSugar.MatchMake* <br>
*Class: MatchMakeManager* </small>

public bool isLocalUserId(UserId id)<br>
public bool isLocalUserId(string id)


### Description
Whether the argument is the id of local user or not.<br>


```cs
using UnityEngine;
using UnityEngine.UI;
using SynicSugar.MatchMake;
using SynicSugar.P2P;
using Cysharp.Threading.Tasks;

public class MatchMakeSample : MonoBehaviour {
    public Button Kick;
    public void SetData(UserId id){
        if(MatchMakeManager.Instance.isHost && !MatchMakeManager.Instance.isLocalUserId(id)){
            Kick.gameObject.SetActive(true);
            Kick.onClick.AddListener(() => MatchMakeManager.Instance.KickTargetFromLobby(id).Forget());
        }
    }
}
```