+++
title = "LocalUserId"
weight = 0
+++
## LocalUserId
<small>*Namespace: SynicSugar.P2P* <br>
*Class: UserIds* </small>

public UserId LocalUserId;


### Description
This Local's UserID. This Id are assigned by EOS.<br>


```cs
using SynicSugar.P2P;

public class p2pSample {
    void UserIDsSample(){
        UserID id = p2pInfo.Instance.LocalUserId;
    }
}
```