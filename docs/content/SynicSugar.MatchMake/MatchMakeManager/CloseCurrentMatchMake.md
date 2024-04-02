+++
title = "CloseCurrentMatchMake"
weight = 14
+++
## CloseCurrentMatchMake
<small>*Namespace: SynicSugar.MatchMake* <br>
*Class: MatchMakeManager* </small>

public async UniTask&lt;bool&gt; CloseCurrentMatchMake(bool destroyManager = true, CancellationToken token = default(CancellationToken))


### Description
Host destroys lobby to cancel matchmake. After Host calls this, all Guests
When Guest call this, just leave lobby.<br>
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

        bool isSuccess = await MatchMakeManager.Instance.CloseCurrentMatchMake(true, cancelToken);
        
        if(isSuccess){
            //Success
            return;
        }
        //Failuer
    }
}
```