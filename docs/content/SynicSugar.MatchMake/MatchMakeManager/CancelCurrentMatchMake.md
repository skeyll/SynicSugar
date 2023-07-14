+++
title = "CancelCurrentMatchMake"
weight = 14
+++
## CancelCurrentMatchMake
<small>*Namespace: SynicSugar.MatchMake* <br>
*Class: MatchMakeManager* </small>

public async UniTask&lt;bool&gt; CancelCurrentMatchMake(bool removeManager = false, CancellationToken token = default(CancellationToken))


### Description
Exit lobby and cancel matchmake.<br>
This can only be called in mathmaking. SynicSugar judges in matchmaking or not based on the cancel token that we passed Create/SearchXXX.<br>
If can, return true.<br>
If we pass true to 1st arg, this will destroy ConnectManager after being able to cancel matchmaking. When we have the two scene for using ConnectManager and not, pass true.<br>
2nd arg is for this Task. (It's not for MatchMaking but we can also use the same token.)<br>


```cs
using UnityEngine;
using System.Threading;
using Cysharp.Threading.Tasks;
using SynicSugar.MatchMake;

public class MatchMake : MonoBehaviour {
    public async void CancelMatchMaking(){
        CancellationTokenSource cts = new CancellationTokenSource();
        CancellationToken cancelToken = matchCancellToken.Token;

        bool isSuccess = await MatchMakeManager.Instance.CancelCurrentMatchMake(true, cancelToken);
        
        if(isSuccess){
            //Success
            return;
        }
        //Failuer
    }
}
```