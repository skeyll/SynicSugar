+++
title = "KickTargetFromLobby"
weight = 24
+++
## KickTargetFromLobby
<small>*Namespace: SynicSugar.MatchMake* <br>
*Class: MatchMakeManager* </small>

public async UniTask&lt;bool&gt; KickTargetFromLobby(UserId targetId, CancellationToken token = default(CancellationToken))


### Description
Host kicks a specific target from Lobby. Only one tareget can be kicked at a time.<br>

This is valid only when we pass **minLobbyMember** to *[MatchMakingGUIEvents](../MatchMakeManager/matchmakingguievents)*, *[SearchLobby](../MatchMakeManager/searchlobby)*, *[CreateLobby](../MatchMakeManager/createlobby)*.<br>
We can display the GUI Button to invoke this with *[MatchMakingGUIEvents](../MatchMakeManager/matchmakingguievents)*.


```cs
using Cysharp.Threading.Tasks;
using SynicSugar.MatchMake;
using SynicSugar.P2P;

public class MatchMakeSample {
    public async UniTask KickMember(UserId target){
        await MatchMakeManager.Instance.KickTargetFromLobby(target);
    }
}
```