+++
title = "CancelCurrentMatchMake"
weight = 14
+++
## CancelCurrentMatchMake
<small>*Namespace: SynicSugar.MatchMake* <br>
*Class: MatchMakeManager* </small>

public async UniTask&lt;bool&gt; CancelCurrentMatchMake(CancellationTokenSource token)


### Description
Exit lobby and cancel MatchMaking.<br>
If can leave matchmaking, return true. Even if gets False, the lobby will eventually be closed and we can ignore this result.<br>
This args' token is for this Task. (not for MatchMaking but we can use same token.)


```cs
using System.Threading;
using Cysharp.Threading.Tasks;
using SynicSugar.MatchMake;

public class MatchMake : MonoBehaviour {
    async void Start(){

        matchCancellToken = new CancellationTokenSource();
        bool isSuccess = await MatchMakeManager.Instance.CancelCurrentMatchMake(matchCancellToken);
        
        if(isSuccess){
            //Success
            return;
        }
        //Failuer
    }
}
```